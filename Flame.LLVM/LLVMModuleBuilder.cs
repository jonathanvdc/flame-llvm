using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Creates a module builder from the given module.
        /// </summary>
        public LLVMModuleBuilder(LLVMModuleRef Module)
        {
            this.module = Module;
            this.declaredMethods = new Dictionary<LLVMMethod, LLVMValueRef>();
            this.declaredTypes = new Dictionary<IType, LLVMTypeRef>();
        }

        private LLVMModuleRef module;
        private Dictionary<LLVMMethod, LLVMValueRef> declaredMethods;
        private Dictionary<IType, LLVMTypeRef> declaredTypes;

        /// <summary>
        /// Declares the given method if it was not declared already.
        /// A value that corresponds to the declaration is returned.
        /// </summary>
        /// <param name="Method">The method to declare.</param>
        /// <returns>An LLVM function.</returns>
        public LLVMValueRef Declare(LLVMMethod Method)
        {
            LLVMValueRef result;
            if (!declaredMethods.TryGetValue(Method, out result))
            {
                var abi = Method.Abi;
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
        private LLVMTypeRef DeclareFunctionType(LLVMMethod Method)
        {
            var paramArr = Method.GetParameters();
            var paramTypes = new LLVMTypeRef[paramArr.Length > 0 ? paramArr.Length : 1];
            for (int i = 0; i < paramArr.Length; i++)
            {
                paramTypes[i] = Declare(paramArr[i].ParameterType);
            }
            return FunctionType(
                Declare(Method.ReturnType),
                out paramTypes[0],
                (uint)paramArr.Length,
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

        private LLVMTypeRef DeclareTypeImpl(IType Type)
        {
            if (Type.GetIsInteger() || Type.GetIsBit())
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
            else
            {
                throw new NotImplementedException("Only primitive types have been implemented so far");
            }
        }
    }
}

