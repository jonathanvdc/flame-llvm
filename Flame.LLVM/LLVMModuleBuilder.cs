using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Variables;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// A wrapper around an LLVM module.
    /// </summary>
    public sealed class LLVMModuleBuilder
    {
        /// <summary>
        /// Creates a module builder from the given assembly and module.
        /// </summary>
        public LLVMModuleBuilder(LLVMAssembly Assembly, LLVMModuleRef Module)
        {
            this.assembly = Assembly;
            this.module = Module;
            this.declaredMethods = new Dictionary<IMethod, LLVMValueRef>();
            this.declaredPrototypes = new Dictionary<IMethod, LLVMTypeRef>();
            this.declaredTypes = new Dictionary<IType, LLVMTypeRef>();
            this.declaredDataLayouts = new Dictionary<LLVMType, LLVMTypeRef>();
            this.declaredGlobals = new Dictionary<IField, LLVMValueRef>();
            this.declaredTypeIds = new Dictionary<LLVMType, ulong>();
            this.declaredTypePrimes = new Dictionary<LLVMType, ulong>();
            this.declaredTypeIndices = new Dictionary<LLVMType, ulong>();
            this.declaredVTables = new Dictionary<LLVMType, VTableInstance>();
            this.interfaceStubs = new Dictionary<LLVMMethod, InterfaceStub>();
            this.primeGen = new PrimeNumberGenerator();
        }

        private LLVMAssembly assembly;
        private LLVMModuleRef module;
        private Dictionary<IMethod, LLVMValueRef> declaredMethods;
        private Dictionary<IMethod, LLVMTypeRef> declaredPrototypes;
        private Dictionary<IType, LLVMTypeRef> declaredTypes;
        private Dictionary<IField, LLVMValueRef> declaredGlobals;
        private Dictionary<LLVMType, LLVMTypeRef> declaredDataLayouts;
        private Dictionary<LLVMType, ulong> declaredTypeIds;
        private Dictionary<LLVMType, ulong> declaredTypePrimes;
        private Dictionary<LLVMType, ulong> declaredTypeIndices;
        private Dictionary<LLVMType, VTableInstance> declaredVTables;
        private Dictionary<LLVMMethod, InterfaceStub> interfaceStubs;
        private PrimeNumberGenerator primeGen;

        /// <summary>
        /// Declares the given method if it was not declared already.
        /// A value that corresponds to the declaration is returned.
        /// </summary>
        /// <param name="Method">The method to declare.</param>
        /// <returns>An LLVM function.</returns>
        public LLVMValueRef Declare(IMethod Method)
        {
            LLVMValueRef result;
            if (!declaredMethods.TryGetValue(Method, out result))
            {
                if (!TryDeclareIntrinsic(Method, out result))
                {
                    var abi = LLVMSymbolTypeMember.GetLLVMAbi(Method, assembly);
                    var funcType = DeclarePrototype(Method);
                    result = AddFunction(module, abi.Mangler.Mangle(Method), funcType);
                }
                declaredMethods[Method] = result;
            }
            return result;
        }

        /// <summary>
        /// Declares the function type for the given method in the given module.
        /// </summary>
        /// <param name="Method">The method to find a function type for.</param>
        /// <param name="Module">The module that declares the function.</param>
        /// <returns>A function type.</returns>
        public LLVMTypeRef DeclarePrototype(IMethod Method)
        {
            LLVMTypeRef result;
            if (!declaredPrototypes.TryGetValue(Method, out result))
            {
                var abi = LLVMSymbolTypeMember.GetLLVMAbi(Method, assembly);
                var paramArr = Method.GetParameters();
                int thisParamCount = Method.IsStatic ? 0 : 1;
                var extParamTypes = new LLVMTypeRef[paramArr.Length > 0 ? thisParamCount + paramArr.Length : 1];
                if (!Method.IsStatic)
                {
                    extParamTypes[0] = PointerType(Int8Type(), 0);
                }
                for (int i = 0; i < paramArr.Length; i++)
                {
                    extParamTypes[i + thisParamCount] = Declare(paramArr[i].ParameterType);
                }
                result = FunctionType(
                    Declare(Method.ReturnType),
                    out extParamTypes[0],
                    (uint)(paramArr.Length + thisParamCount),
                    false);
                declaredPrototypes[Method] = result;
            }
            return result;
        }

        /// <summary>
        /// Tries to declare the given method as an intrinsic.
        /// </summary>
        /// <param name="Method">The method to declare.</param>
        /// <param name="Result">The resulting declaration.</param>
        /// <returns><c>true</c> if the method is an intrinsic; otherwise, <c>false</c>.</returns>
        private bool TryDeclareIntrinsic(IMethod Method, out LLVMValueRef Result)
        {
            if (Method.DeclaringType.GetIsArray())
            {
                if (Method is IAccessor)
                {
                    var arrayAccessor = (IAccessor)Method;
                    if (arrayAccessor.DeclaringProperty.Name.ToString() == "Length"
                        && arrayAccessor.GetIsGetAccessor())
                    {
                        Result = DeclareArrayLength(Method);
                        return true;
                    }
                }
            }
            Result = default(LLVMValueRef);
            return false;
        }

        private LLVMValueRef DeclareArrayLength(IMethod Method)
        {
            var arrayType = Method.DeclaringType.AsArrayType();

            // Declare T[,...].Length.
            var abi = LLVMSymbolTypeMember.GetLLVMAbi(Method, assembly);
            var funcType = DeclarePrototype(Method);
            var funcDef = AddFunction(module, abi.Mangler.Mangle(Method), funcType);
            funcDef.SetLinkage(LLVMLinkage.LLVMWeakODRLinkage);

            // Define T[,...].Length's body.
            var codeGenerator = new Codegen.LLVMCodeGenerator(Method);

            // T[,...].Length computes the product of all dimensions.
            var dimensions = new ICodeBlock[arrayType.ArrayRank];
            for (int i = 0; i < dimensions.Length; i++)
            {
                dimensions[i] = codeGenerator.EmitDereferencePointer(
                    new Codegen.GetDimensionPtrBlock(
                        codeGenerator,
                        (Codegen.CodeBlock)codeGenerator.GetThis().EmitGet(),
                        i));
            }

            var body = (Codegen.CodeBlock)codeGenerator.EmitReturn(codeGenerator.EmitProduct(dimensions));

            // Emit T[,...].Length's body.
            var bodyBuilder = new Codegen.FunctionBodyBuilder(this, funcDef);
            var entryPointBuilder = bodyBuilder.AppendBasicBlock("entry");
            entryPointBuilder = codeGenerator.Prologue.Emit(entryPointBuilder);
            body.Emit(entryPointBuilder);

            return funcDef;
        }

        /// <summary>
        /// Declares the given field if it is static and has not been declared
        /// already. An LLVM value that corresponds to the declaration is returned.
        /// </summary>
        /// <param name="Field">The field to declare.</param>
        /// <returns>An LLVM global.</returns>
        public LLVMValueRef DeclareGlobal(IField Field)
        {
            if (!Field.IsStatic)
            {
                throw new InvalidOperationException(
                    "Instance field '" + Field.Name +
                    "' cannot be declared as a global.");
            }

            LLVMValueRef result;
            if (!declaredGlobals.TryGetValue(Field, out result))
            {
                // Declare the global.
                var abiMangler = LLVMSymbolTypeMember.GetLLVMAbi(Field, assembly).Mangler;
                result = DeclareGlobal(Declare(Field.FieldType), abiMangler.Mangle(Field));

                if (Field is LLVMField)
                {
                    var llvmField = (LLVMField)Field;

                    // Set the field's linkage.
                    result.SetLinkage(llvmField.Linkage);

                    if (!llvmField.IsImport)
                    {
                        // Zero-initialize it.
                        var codeGenerator = new Codegen.LLVMCodeGenerator(null);
                        var defaultValueBlock = (Codegen.CodeBlock)codeGenerator.EmitDefaultValue(Field.FieldType);
                        var defaultValueRef = defaultValueBlock.Emit(
                            new Codegen.BasicBlockBuilder(
                                new Codegen.FunctionBodyBuilder(this, default(LLVMValueRef)),
                                default(LLVMBasicBlockRef)));
                        LLVMSharp.LLVM.SetInitializer(result, defaultValueRef.Value);
                    }
                }

                // Store it in the dictionary.
                declaredGlobals[Field] = result;
            }
            return result;
        }

        /// <summary>
        /// Declares a global with the given type and name.
        /// </summary>
        /// <param name="Type">The global's type.</param>
        /// <param name="Name">The global's name.</param>
        /// <returns>The global value.</returns>
        public LLVMValueRef DeclareGlobal(LLVMTypeRef Type, string Name)
        {
            return AddGlobal(module, Type, Name);
        }

        /// <summary>
        /// Declares the data layout of the given type if it was not declared already.
        /// An LLVM type that corresponds to the declaration is returned.
        /// </summary>
        /// <param name="Type">The type to declare.</param>
        /// <returns>An LLVM type.</returns>
        public LLVMTypeRef DeclareDataLayout(LLVMType Type)
        {
            LLVMTypeRef result;
            if (!declaredDataLayouts.TryGetValue(Type, out result))
            {
                if (Type.GetIsValueType())
                {
                    // The layout of value types can be computed naively.
                    result = Type.DefineLayout(this);
                    declaredDataLayouts[Type] = result;
                }
                else
                {
                    // Reference types cannot have their layout computed naively, because
                    // they might refer to themselves. To get around this, we first declare
                    // a named struct, then figure out what its layout should look like and
                    // finally fill it out.
                    result = StructCreateNamed(GetGlobalContext(), Type.FullName.ToString());
                    declaredDataLayouts[Type] = result;
                    var layout = Type.DefineLayout(this);
                    StructSetBody(result, GetStructElementTypes(layout), IsPackedStruct(layout));
                }
            }
            return result;
        }

        /// <summary>
        /// Declares the given type if it was not declared already.
        /// An LLVM type that corresponds to the declaration is returned.
        /// </summary>
        /// <param name="Type">The type to declare.</param>
        /// <returns>An LLVM type.</returns>
        public LLVMTypeRef Declare(IType Type)
        {
            LLVMTypeRef result;
            if (!declaredTypes.TryGetValue(Type, out result))
            {
                result = DeclareTypeImpl(Type);
                declaredTypes[Type] = result;
            }
            return result;
        }

        private LLVMTypeRef DeclareTypeImpl(IType Type)
        {
            if (Type.GetIsPointer())
            {
                var elemType = Type.AsPointerType().ElementType;
                if (elemType == PrimitiveTypes.Void)
                    return LLVMSharp.LLVM.PointerType(IntType(8), 0);
                else
                    return LLVMSharp.LLVM.PointerType(Declare(elemType), 0);
            }
            else if (Type.GetIsArray())
            {
                // We'll lay out arrays like so:
                //
                //     { i32, ..., [0 x <type>] }
                //
                // where the first fields are the dimensions and the last field
                // is the data. When we allocate an array, we'll allocate the
                // right amount of tail room for the data by allocating
                // `sizeof(i32, ..., [0 x <type>])` bytes.

                var elemType = Type.AsArrayType().ElementType;
                var fields = new LLVMTypeRef[Type.AsArrayType().ArrayRank + 1];
                for (int i = 0; i < fields.Length - 1; i++)
                {
                    fields[i] = Int32Type();
                }
                fields[fields.Length - 1] = ArrayType(Declare(elemType), 0);
                return LLVMSharp.LLVM.PointerType(
                    StructType(fields, false),
                    0);
            }
            else if (Type.GetIsInteger() || Type.GetIsBit())
            {
                return IntType((uint)Type.GetPrimitiveBitSize());
            }
            else if (Type == PrimitiveTypes.Float32)
            {
                return FloatType();
            }
            else if (Type == PrimitiveTypes.Float64)
            {
                return DoubleType();
            }
            else if (Type == PrimitiveTypes.Void)
            {
                return VoidType();
            }
            else if (Type == PrimitiveTypes.Char)
            {
                return IntType(16);
            }
            else if (Type == PrimitiveTypes.Boolean)
            {
                return Int1Type();
            }
            else if (Type is LLVMType)
            {
                var llvmType = (LLVMType)Type;
                if (llvmType.GetIsValueType())
                {
                    return DeclareDataLayout(llvmType);
                }
                else if (llvmType.GetIsReferenceType())
                {
                    return PointerType(DeclareDataLayout(llvmType), 0);
                }
            }
            else if (Type.GetIsReferenceType())
            {
                // We don't know what the reference type's layout is, so the
                // best we can do is to just create a void pointer.
                return PointerType(Int8Type(), 0);
            }
            throw new NotImplementedException(string.Format("Type not supported: '{0}'", Type));
        }

        /// <summary>
        /// Gets the type ID for the given type.
        /// </summary>
        /// <param name="Type">The type for which a type ID is to be found.</param>
        /// <returns>An type ID.</returns>
        public ulong GetTypeId(LLVMType Type)
        {
            ulong result;
            if (!declaredTypeIds.TryGetValue(Type, out result))
            {
                GenerateTypeId(Type);
                result = declaredTypeIds[Type];
            }
            return result;
        }

        /// <summary>
        /// Gets the prime number associated with the given type.
        /// </summary>
        /// <param name="Type">The type for which a prime number is to be found.</param>
        /// <returns>A prime number.</returns>
        public ulong GetTypePrime(LLVMType Type)
        {
            ulong result;
            if (!declaredTypePrimes.TryGetValue(Type, out result))
            {
                GenerateTypeId(Type);
                result = declaredTypePrimes[Type];
            }
            return result;
        }

        /// <summary>
        /// Generates an type ID for the given type.
        /// </summary>
        /// <param name="Type">The type for which a type ID is to be generated.</param>
        /// <returns>An type ID.</returns>
        private void GenerateTypeId(LLVMType Type)
        {
            // The notion of a type ID is based on "Fast Dynamic Casting" by
            // Michael Gibbs and Bjarne Stroustrup.
            // (http://www.rajatorrent.com.stroustrup.com/fast_dynamic_casting.pdf)
            //
            // The idea is that every type has a unique ID and that this ID is the
            // product of
            //
            //     1. a unique prime number for the type, and
            //
            //     2. the prime numbers for all of its base types.
            //
            // We can then easily test if a value X is an instance of type T by
            // checking if
            //
            //     typeid(X) % typeid(T) == 0.

            // TODO: be smarter about how type IDs are generated. The paper says
            //
            //     There are a number of heuristic methods for keeping the size of the
            //     type ID to a minimum number of bits. To prevent the type IDs from
            //     being any larger than necessary, the classes are sorted according
            //     to priority. The priority for a class is the maximum number of
            //     ancestors that any of its descendants has. The priority reflects
            //     the number of prime factors that are multiplied together to form a
            //     class' type ID. Classes with the highest priority are assigned the
            //     smallest prime numbers.
            //
            //     If we had to assign each class a unique prime number, the type IDs
            //     would quickly get very large. However, this is not strictly
            //     necessary. While all classes at the root level (those having no base
            //     classes) must be assigned globally unique prime numbers, independent
            //     hierarchies can use the same primes for non-root classes without
            //     conflict. Two classes with a common descendant then cannot have the
            //     same prime and none of their children may have the same prime. This
            //     also implies that no two children of a class may have the same prime.
            //     In all other cases, the primes can be duplicated across a level of
            //     the hierarchy. For example, in a tree structure two classes on the
            //     same level of the tree never have a common descendant, so they
            //     may have identical sub-trees beneath them without a conflict.

            var allBasePrimes = Type
                .GetAllBaseTypes()
                .Cast<LLVMType>()
                .Distinct<LLVMType>()
                .Select<LLVMType, ulong>(GetTypePrime)
                .ToArray<ulong>();
            ulong id = 1;
            foreach (var basePrime in allBasePrimes)
            {
                id *= basePrime;
            }

            ulong prime = primeGen.Next();
            declaredTypePrimes[Type] = prime;
            id *= prime;
            
            declaredTypeIds[Type] = id;
        }

        /// <summary>
        /// Gets the index associated with the given type.
        /// </summary>
        /// <param name="Type">The type for which the index is to be found.</param>
        /// <returns>An integer index.</returns>
        public ulong GetTypeIndex(LLVMType Type)
        {
            ulong result;
            if (!declaredTypeIndices.TryGetValue(Type, out result))
            {
                result = (ulong)declaredTypeIndices.Count + 1;
                declaredTypeIndices[Type] = result;
            }
            return result;
        }

        /// <summary>
        /// Gets the vtable global for the given type.
        /// </summary>
        /// <param name="Type">The type to get the vtable of.</param>
        /// <returns>A vtable global.</returns>
        public VTableInstance GetVTable(LLVMType Type)
        {
            VTableInstance vtable;
            if (!declaredVTables.TryGetValue(Type, out vtable))
            {
                vtable = Type.DefineVTable(this);
                declaredVTables[Type] = vtable;
            }
            return vtable;
        }

        /// <summary>
        /// Gets the interface stub for the given method.
        /// </summary>
        /// <param name="Method">The method to get the interface stub of.</param>
        /// <returns>An interface stub.</returns>
        public InterfaceStub GetInterfaceStub(LLVMMethod Method)
        {
            InterfaceStub stub;
            if (!interfaceStubs.TryGetValue(Method, out stub))
            {
                var retType = PointerType(DeclarePrototype(Method), 0);
                var paramTypes = new LLVMTypeRef[] { Int64Type() };
                var stubFunc = AddFunction(
                    module,
                    Method.Abi.Mangler.Mangle(Method) + "_stub",
                    FunctionType(retType, paramTypes, false));
                stubFunc.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

                stub = new InterfaceStub(stubFunc);
                interfaceStubs[Method] = stub;
            }
            return stub;
        }

        /// <summary>
        /// Defines the bodies of interface stubs.
        /// </summary>
        public void EmitStubs()
        {
            foreach (var pair in interfaceStubs)
            {
                pair.Value.Emit(this);
            }
        }
    }
}

