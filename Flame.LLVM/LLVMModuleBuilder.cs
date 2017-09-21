using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Attributes;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Variables;
using Flame.LLVM.Codegen;
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
            this.declaredVirtMethods = new Dictionary<IMethod, LLVMValueRef>();
            this.declaredPrototypes = new Dictionary<IMethod, LLVMTypeRef>();
            this.declaredTypes = new Dictionary<IType, LLVMTypeRef>();
            this.declaredDataLayouts = new Dictionary<LLVMType, LLVMTypeRef>();
            this.declaredGlobals = new Dictionary<IField, LLVMValueRef>();
            this.declaredTypeIds = new Dictionary<IType, ulong>();
            this.declaredTypePrimes = new Dictionary<IType, ulong>();
            this.declaredTypeIndices = new Dictionary<IType, ulong>();
            this.declaredVTables = new Dictionary<IType, VTableInstance>();
            this.declaredIntrinsics = new Dictionary<IntrinsicValue, LLVMValueRef>();
            this.interfaceStubs = new Dictionary<LLVMMethod, InterfaceStub>();
            this.primeGen = new PrimeNumberGenerator();
            this.staticConstructorLocks = new Dictionary<LLVMType, Tuple<LLVMValueRef, LLVMValueRef, LLVMValueRef>>();
            this.declaredStringChars = new Dictionary<string, LLVMValueRef>();
        }

        private LLVMAssembly assembly;
        private LLVMModuleRef module;
        private Dictionary<IMethod, LLVMValueRef> declaredMethods;
        private Dictionary<IMethod, LLVMValueRef> declaredVirtMethods;
        private Dictionary<IMethod, LLVMTypeRef> declaredPrototypes;
        private Dictionary<IType, LLVMTypeRef> declaredTypes;
        private Dictionary<IField, LLVMValueRef> declaredGlobals;
        private Dictionary<LLVMType, LLVMTypeRef> declaredDataLayouts;
        private Dictionary<IType, ulong> declaredTypeIds;
        private Dictionary<IType, ulong> declaredTypePrimes;
        private Dictionary<IType, ulong> declaredTypeIndices;
        private Dictionary<IType, VTableInstance> declaredVTables;
        private Dictionary<LLVMMethod, InterfaceStub> interfaceStubs;
        private Dictionary<IntrinsicValue, LLVMValueRef> declaredIntrinsics;
        private PrimeNumberGenerator primeGen;
        private Dictionary<LLVMType, Tuple<LLVMValueRef, LLVMValueRef, LLVMValueRef>> staticConstructorLocks;
        private Dictionary<string, LLVMValueRef> declaredStringChars;

        private static readonly IntrinsicAttribute NoAliasAttribute =
            new IntrinsicAttribute("NoAliasAttribute");

        private static readonly IntrinsicAttribute NoThrowAttribute =
            new IntrinsicAttribute("NoThrowAttribute");

        private static readonly IntrinsicAttribute ReadNoneAttribute =
            new IntrinsicAttribute("ReadNoneAttribute");

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
                    string methodName = abi.Mangler.Mangle(Method, true);
                    result = GetNamedFunction(module, methodName);
                    if (result.Pointer == IntPtr.Zero
                        || !Method.HasAttribute(
                            PrimitiveAttributes.Instance.ImportAttribute.AttributeType))
                    {
                        // Only declare imported methods if they haven't been declared
                        // already.
                        //
                        // It's tempting to think that a valid assembly declares
                        // methods only once, that's not true: sometimes, the same
                        // function is imported multiple times by different classes.
                        var funcType = DeclarePrototype(Method);
                        result = AddFunction(module, methodName, funcType);
                        if (Method.HasAttribute(NoAliasAttribute.AttributeType))
                        {
                            AddAttributeAtIndex(
                                result,
                                LLVMAttributeIndex.LLVMAttributeReturnIndex,
                                CreateEnumAttribute("noalias"));
                        }
                        if (Method.HasAttribute(NoThrowAttribute.AttributeType))
                        {
                            AddAttributeAtIndex(
                                result,
                                LLVMAttributeIndex.LLVMAttributeFunctionIndex,
                                CreateEnumAttribute("nounwind"));
                        }
                        if (Method.HasAttribute(ReadNoneAttribute.AttributeType))
                        {
                            AddAttributeAtIndex(
                                result,
                                LLVMAttributeIndex.LLVMAttributeFunctionIndex,
                                CreateEnumAttribute("readnone"));
                        }
                    }
                }
                declaredMethods[Method] = result;
            }
            return result;
        }

        private LLVMAttributeRef CreateEnumAttribute(string Name)
        {
            uint kind = GetEnumAttributeKindForName(Name, new size_t((IntPtr)Name.Length));
            return LLVMSharp.LLVM.CreateEnumAttribute(LLVMSharp.LLVM.GetModuleContext(module), kind, 0);
        }

        /// <summary>
        /// Declares the given method if it was not declared already.
        /// A function pointer is returned that can be used for virtual
        /// function calls.
        /// </summary>
        /// <param name="Method">The method to declare.</param>
        /// <returns>An LLVM function.</returns>
        public LLVMValueRef DeclareVirtual(IMethod Method)
        {
            LLVMValueRef result;
            if (!declaredVirtMethods.TryGetValue(Method, out result))
            {
                var declMethod = Declare(Method);
                if (Method.DeclaringType.GetIsValueType())
                {
                    // Boxed value types are represented as `{ byte*, T }` structs, but
                    // T's methods expect a `T*` as first parameter. So we can't fill
                    // T's vtable/interface stubs with T's methods, because a method
                    // from a vtable/interface stub will be called on a boxed value.
                    //
                    // Instead, we'll create a bunch of thunks and store those in the
                    // vtable. These thunks simply unbox a boxed value and then call a
                    // method on the unboxed pointer.

                    var funcType = declMethod.TypeOf().GetElementType();

                    // Create a thunk.
                    result = AddFunction(
                        module,
                        assembly.Abi.Mangler.Mangle(Method, true) + "_vthunk",
                        funcType);

                    declaredVirtMethods[Method] = result;

                    // Make it internal.
                    result.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

                    // Write its function body.
                    var entryBlock = result.AppendBasicBlock("entry");
                    var builder = CreateBuilder();
                    PositionBuilderAtEnd(builder, entryBlock);

                    var boxPtr = BuildBitCast(
                        builder,
                        GetParam(result, 0),
                        Declare(Method.DeclaringType.MakePointerType(PointerKind.BoxPointer)),
                        "box_ptr");

                    var unboxedPtr = BuildStructGEP(
                        builder,
                        boxPtr,
                        1,
                        "unboxed_ptr");

                    var paramTypes = funcType.GetParamTypes();
                    var args = new LLVMValueRef[paramTypes.Length];
                    args[0] = BuildBitCast(
                        builder,
                        unboxedPtr,
                        paramTypes[0],
                        "this_ptr");

                    for (uint i = 1; i < args.Length; i++)
                    {
                        args[i] = GetParam(result, i);
                    }

                    bool returnsVoid = Method.ReturnType == PrimitiveTypes.Void;
                    var callVal = BuildCall(builder, declMethod, args, returnsVoid ? null : "call_tmp");
                    if (returnsVoid)
                    {
                        BuildRetVoid(builder);
                    }
                    else
                    {
                        BuildRet(builder, callVal);
                    }

                    DisposeBuilder(builder);
                }
                else
                {
                    result = declMethod;
                    declaredVirtMethods[Method] = result;
                }
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
            var funcDef = AddFunction(module, abi.Mangler.Mangle(Method, true), funcType);
            funcDef.SetLinkage(LLVMLinkage.LLVMWeakODRLinkage);
            AddAttributeAtIndex(
                funcDef,
                LLVMAttributeIndex.LLVMAttributeFunctionIndex,
                CreateEnumAttribute("nounwind"));

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
                result = DeclareGlobal(Declare(Field.FieldType), abiMangler.Mangle(Field, true));

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
                if (Type.GetIsValueType() || Type.GetIsEnum() || Type.IsRuntimeImplementedDelegate)
                {
                    // The layout of some types can be computed naively.
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
                var ptrType = Type.AsPointerType();
                var elemType = ptrType.ElementType;
                if (elemType == PrimitiveTypes.Void)
                {
                    return LLVMSharp.LLVM.PointerType(Int8Type(), 0);
                }
                else if (ptrType.PointerKind.Equals(PointerKind.BoxPointer))
                {
                    // A boxed T is represented as a `{ byte*, T }*`.
                    return LLVMSharp.LLVM.PointerType(
                        StructType(
                            new LLVMTypeRef[]
                            {
                                LLVMSharp.LLVM.PointerType(Int8Type(), 0),
                                Declare(elemType)
                            },
                            false),
                        0);
                }
                else
                {
                    return LLVMSharp.LLVM.PointerType(Declare(elemType), 0);
                }
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
                if (llvmType.GetIsValueType() || llvmType.GetIsEnum())
                {
                    return DeclareDataLayout(llvmType);
                }
                else if (llvmType.GetIsReferenceType())
                {
                    return PointerType(DeclareDataLayout(llvmType), 0);
                }
            }
            else if (Type is MethodType)
            {
                return PointerType(DelegateBlock.MethodTypeLayout, 0);
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
        /// Declares the given intrinsic if it was not declared already.
        /// </summary>
        /// <param name="Intrinsic">The intrinsic to declare.</param>
        /// <returns>The intrinsic's declaration.</returns>
        public LLVMValueRef Declare(IntrinsicValue Intrinsic)
        {
            LLVMValueRef result;
            if (!declaredIntrinsics.TryGetValue(Intrinsic, out result))
            {
                result = Intrinsic.Declare(this, module);
                declaredIntrinsics[Intrinsic] = result;
            }
            return result;
        }

        /// <summary>
        /// Gets the type ID for the given type.
        /// </summary>
        /// <param name="Type">The type for which a type ID is to be found.</param>
        /// <returns>An type ID.</returns>
        public ulong GetTypeId(IType Type)
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
        public ulong GetTypePrime(IType Type)
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
        private void GenerateTypeId(IType Type)
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
        public ulong GetTypeIndex(IType Type)
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
        public VTableInstance GetVTable(IType Type)
        {
            VTableInstance vtable;
            if (!declaredVTables.TryGetValue(Type, out vtable))
            {
                if (Type is LLVMType)
                {
                    vtable = ((LLVMType)Type).DefineVTable(this);
                }
                else
                {
                    vtable = new VTableInstance(
                        LLVMType.DefineVTableGlobal(this, Type, new LLVMValueRef[] { }),
                        new LLVMMethod[] { });
                }
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
                    Method.Abi.Mangler.Mangle(Method, true) + "_stub",
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

        /// <summary>
        /// Generates code that runs the given type's static constructors.
        /// </summary>
        /// <param name="BasicBlock">The basic block to extend.</param>
        /// <param name="Type">The type whose static constructors are to be run.</param>
        /// <returns>A basic block builder.</returns>
        public BasicBlockBuilder EmitRunStaticConstructors(BasicBlockBuilder BasicBlock, LLVMType Type)
        {
            if (Type.StaticConstructors.Count == 0)
            {
                return BasicBlock;
            }

            // Get or create the lock triple for the type. A lock triple consists of:
            //
            //     * A global Boolean flag that tells if all static constructors for
            //       the type have been run.
            //
            //     * A global Boolean flag that tells if the static constructors for
            //       the type are in the process of being run.
            //
            //     * A thread-local Boolean flag that tells if the static constructors
            //       for the type are in the process of being run **by the current
            //       thread.**
            //
            Tuple<LLVMValueRef, LLVMValueRef, LLVMValueRef> lockTriple;
            if (!staticConstructorLocks.TryGetValue(Type, out lockTriple))
            {
                string typeName = Type.Namespace.Assembly.Abi.Mangler.Mangle(Type, true);

                var hasRunGlobal = AddGlobal(module, Int8Type(), typeName + "__has_run_cctor");
                hasRunGlobal.SetInitializer(ConstInt(Int8Type(), 0, false));
                hasRunGlobal.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

                var isRunningGlobal = AddGlobal(module, Int8Type(), typeName + "__is_running_cctor");
                isRunningGlobal.SetInitializer(ConstInt(Int8Type(), 0, false));
                isRunningGlobal.SetLinkage(LLVMLinkage.LLVMInternalLinkage);

                var isRunningHereGlobal = AddGlobal(module, Int8Type(), typeName + "__is_running_cctor_here");
                isRunningHereGlobal.SetInitializer(ConstInt(Int8Type(), 0, false));
                isRunningHereGlobal.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
                isRunningHereGlobal.SetThreadLocal(true);

                lockTriple = new Tuple<LLVMValueRef, LLVMValueRef, LLVMValueRef>(
                    hasRunGlobal, isRunningGlobal, isRunningHereGlobal);
                staticConstructorLocks[Type] = lockTriple;
            }

            // A type's static constructors need to be run *before* any of its static fields
            // are accessed. Also, we need to ensure that a type's static constructors are
            // run only *once* in both single-threaded and multi-threaded environments.
            //
            // We'll generate something along the lines of this when a type's static
            // constructors need to be run:
            //
            //     if (!has_run_cctor && !is_running_cctor_here)
            //     {
            //         while (!Interlocked.CompareExchange(ref is_running_cctor, false, true)) { }
            //         if (!has_run_cctor)
            //         {
            //             cctor();
            //             has_run_cctor = true;
            //         }
            //         is_running_cctors = false;
            //     }
            //
            // Or, using gotos:
            //
            //         if (has_run_cctor) goto after_cctor;
            //         else goto check_running_cctors_here;
            //
            //     check_running_cctor_here:
            //         if (is_running_cctors_here) goto after_cctor;
            //         else goto wait_for_cctor_lock;
            //
            //     wait_for_cctor_lock:
            //         is_running_cctors_here = true;
            //         has_exchanged = Interlocked.CompareExchange(ref is_running_cctos, false, true);
            //         if (has_exchanged) goto maybe_run_cctor;
            //         else goto wait_for_cctor_lock;
            //
            //     maybe_run_cctor:
            //         if (has_run_cctor) goto reset_lock;
            //         else goto run_cctor;
            //
            //     run_cctor:
            //         cctor();
            //         has_run_cctor = true;
            //         goto reset_lock;
            //
            //     reset_lock:
            //         is_running_cctors = false;
            //         goto after_cctor;
            //
            //     after_cctor:
            //         ...

            var hasRunPtr = lockTriple.Item1;
            var isRunningPtr = lockTriple.Item2;
            var isRunningHerePtr = lockTriple.Item3;

            var checkRunningHereBlock = BasicBlock.CreateChildBlock("check_running_cctor_here");
            var waitForLockBlock = BasicBlock.CreateChildBlock("wait_for_cctor_lock");
            var maybeRunBlock = BasicBlock.CreateChildBlock("maybe_run_cctor");
            var runBlock = BasicBlock.CreateChildBlock("run_cctor");
            var resetLockBlock = BasicBlock.CreateChildBlock("reset_lock");
            var afterBlock = BasicBlock.CreateChildBlock("after_cctor");

            // Implement the entry basic block.
            BuildCondBr(
                BasicBlock.Builder,
                IntToBoolean(
                    BasicBlock.Builder,
                    BuildAtomicLoad(BasicBlock.Builder, hasRunPtr, "has_run_cctor")),
                afterBlock.Block,
                checkRunningHereBlock.Block);

            // Implement the `check_running_cctor_here` block.
            BuildCondBr(
                checkRunningHereBlock.Builder,
                IntToBoolean(
                    checkRunningHereBlock.Builder,
                    BuildLoad(checkRunningHereBlock.Builder, isRunningHerePtr, "is_running_cctor_here")),
                afterBlock.Block,
                waitForLockBlock.Block);

            // Implement the `wait_for_cctor_lock` block.
            BuildStore(waitForLockBlock.Builder, ConstInt(Int8Type(), 1, false), isRunningHerePtr);

            var cmpxhg = BuildAtomicCmpXchg(
                waitForLockBlock.Builder,
                isRunningPtr,
                ConstInt(Int8Type(), 0, false),
                ConstInt(Int8Type(), 1, false),
                LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                LLVMAtomicOrdering.LLVMAtomicOrderingMonotonic,
                false);
            cmpxhg.SetValueName("is_running_cctor_cmpxhg");

            BuildCondBr(
                waitForLockBlock.Builder,
                BuildExtractValue(
                    waitForLockBlock.Builder,
                    cmpxhg,
                    1,
                    "has_exchanged"),
                maybeRunBlock.Block,
                waitForLockBlock.Block);

            // Implement the `maybe_run_cctor` block.
            BuildCondBr(
                maybeRunBlock.Builder,
                IntToBoolean(
                    maybeRunBlock.Builder,
                    BuildAtomicLoad(maybeRunBlock.Builder, hasRunPtr, "has_run_cctor")),
                resetLockBlock.Block,
                runBlock.Block);

            // Implement the `run_cctor` block.
            for (int i = 0; i < Type.StaticConstructors.Count; i++)
            {
                BuildCall(runBlock.Builder, Declare(Type.StaticConstructors[i]), new LLVMValueRef[] { }, "");
            }
            BuildAtomicStore(runBlock.Builder, ConstInt(Int8Type(), 1, false), hasRunPtr);
            BuildBr(runBlock.Builder, resetLockBlock.Block);

            // Implement the `reset_lock` block.
            BuildAtomicStore(resetLockBlock.Builder, ConstInt(Int8Type(), 0, false), isRunningPtr);
            BuildBr(resetLockBlock.Builder, afterBlock.Block);

            return afterBlock;
        }

        private static LLVMValueRef BuildAtomicLoad(
            LLVMBuilderRef Builder,
            LLVMValueRef Pointer,
            string Name)
        {
            var instruction = BuildLoad(Builder, Pointer, Name);
            SetOrdering(instruction, LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent);
            SetAlignment(instruction, 1);
            return instruction;
        }

        private static LLVMValueRef BuildAtomicStore(
            LLVMBuilderRef Builder,
            LLVMValueRef Value,
            LLVMValueRef Pointer)
        {
            var instruction = BuildStore(Builder, Value, Pointer);
            SetOrdering(instruction, LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent);
            SetAlignment(instruction, 1);
            return instruction;
        }

        private static LLVMValueRef IntToBoolean(LLVMBuilderRef Builder, LLVMValueRef Value)
        {
            return BuildICmp(
                Builder,
                LLVMIntPredicate.LLVMIntNE,
                Value,
                ConstInt(Int8Type(), 0, false),
                "cmp_result");
        }

        /// <summary>
        /// Gets a constant global character array for the given string.
        /// </summary>
        /// <param name="Value">The string to get a constant global character array for.</param>
        /// <returns>A constant global character array.</returns>
        public LLVMValueRef GetConstCharArray(string Value)
        {
            LLVMValueRef result;
            if (!declaredStringChars.TryGetValue(Value, out result))
            {
                var data = ConstStruct(
                    new LLVMValueRef[]
                    {
                        ConstInt(Int32Type(), (ulong)Value.Length, true),
                        ConstArray(
                            Int16Type(),
                            Enumerable.Select<char, LLVMValueRef>(Value, ToLLVMValue)
                                .ToArray<LLVMValueRef>())
                    },
                    false);
                result = DeclareGlobal(data.TypeOf(), "__string_literal." + declaredStringChars.Count);
                result.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
                result.SetGlobalConstant(true);
                result.SetInitializer(data);
                result = ConstBitCast(
                    result,
                    PointerType(
                        StructType(
                            new LLVMTypeRef[] { Int32Type(), ArrayType(Int16Type(), 0) },
                            false),
                        0));
                declaredStringChars[Value] = result;
            }
            return result;
        }

        private static LLVMValueRef ToLLVMValue(char value)
        {
            return ConstInt(Int16Type(), value, false);
        }
    }
}

