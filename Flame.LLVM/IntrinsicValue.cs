using System;
using LLVMSharp;

namespace Flame.LLVM
{
    /// <summary>
    /// Represents an intrinsic value: a well-known value that is injected into
    /// generated modules.
    /// </summary>
    public sealed class IntrinsicValue
    {
        /// <summary>
        /// Creates an intrinsic from the given intrinsic definition.
        /// </summary>
        /// <param name="Type">The type of the intrinsic.</param>
        /// <param name="Declare">Declares the intrinsic in a module.</param>
        public IntrinsicValue(IType Type, Func<LLVMModuleRef, LLVMValueRef> Declare)
        {
            this.Type = Type;
            this.declareIntrinsic = Declare;
        }

        /// <summary>
        /// Gets the intrinsic's type.
        /// </summary>
        /// <returns>The intrinsic's type.</returns>
        public IType Type { get; private set; }

        private Func<LLVMModuleRef, LLVMValueRef> declareIntrinsic;

        /// <summary>
        /// Declares this intrinsic in the given module.
        /// </summary>
        /// <param name="Module">The module to declare the intrinsic in.</param>
        /// <returns>The intrinsic's declaration.</returns>
        public LLVMValueRef Declare(LLVMModuleRef Module)
        {
            return declareIntrinsic(Module);
        }
    }
}