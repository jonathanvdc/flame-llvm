using System;
using System.Collections.Generic;
using System.Linq;
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
            this.declaredTypes = new Dictionary<IType, LLVMTypeRef>();
            this.declaredGlobals = new Dictionary<IField, LLVMValueRef>();
        }

        private LLVMAssembly assembly;
        private LLVMModuleRef module;
        private Dictionary<IMethod, LLVMValueRef> declaredMethods;
        private Dictionary<IType, LLVMTypeRef> declaredTypes;
        private Dictionary<IField, LLVMValueRef> declaredGlobals;

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
                var abi = LLVMSymbolTypeMember.GetLLVMAbi(Method, assembly);
                var funcType = DeclareFunctionType(Method);
                result = AddFunction(module, abi.Mangler.Mangle(Method), funcType);
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
        private LLVMTypeRef DeclareFunctionType(IMethod Method)
        {
            var paramArr = Method.GetParameters();
            int thisParamCount = Method.IsStatic ? 0 : 1;
            var extParamTypes = new LLVMTypeRef[paramArr.Length > 0 ? thisParamCount + paramArr.Length : 1];
            if (!Method.IsStatic)
            {
                extParamTypes[0] = Declare(ThisVariable.GetThisType(Method.DeclaringType));
            }
            for (int i = 0; i < paramArr.Length; i++)
            {
                extParamTypes[i + thisParamCount] = Declare(paramArr[i].ParameterType);
            }
            return FunctionType(
                Declare(Method.ReturnType),
                out extParamTypes[0],
                (uint)(paramArr.Length + thisParamCount),
                false);
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
                result = AddGlobal(module, Declare(Field.FieldType), abiMangler.Mangle(Field));

                if (Field is LLVMField)
                {
                    var llvmField = (LLVMField)Field;

                    // Set the field's linkage.
                    result.SetLinkage(llvmField.Linkage);

                    // Zero-initialize it.
                    var codeGenerator = new Codegen.LLVMCodeGenerator(null);
                    var defaultValueBlock = (Codegen.CodeBlock)codeGenerator.EmitDefaultValue(Field.FieldType);
                    var defaultValueRef = defaultValueBlock.Emit(
                        new Codegen.BasicBlockBuilder(
                            new Codegen.FunctionBodyBuilder(this, default(LLVMValueRef)),
                            default(LLVMBasicBlockRef)));
                    LLVMSharp.LLVM.SetInitializer(result, defaultValueRef.Value);
                }

                // Store it in the dictionary.
                declaredGlobals[Field] = result;
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
            else if (Type is LLVMType)
            {
                var llvmType = (LLVMType)Type;
                if (llvmType.GetIsValueType())
                {
                    return llvmType.DefineLayout(this);
                }
                else if (llvmType.GetIsReferenceType())
                {
                    return PointerType(llvmType.DefineLayout(this), 0);
                }
            }
            throw new NotImplementedException(string.Format("Type not supported: '{0}'", Type));
        }
    }
}

