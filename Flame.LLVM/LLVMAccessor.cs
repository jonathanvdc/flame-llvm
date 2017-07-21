using System;
using Flame.Compiler.Build;

namespace Flame.LLVM
{
    /// <summary>
    /// An accessor implementation for the LLVM back-end.
    /// </summary>
    public sealed class LLVMAccessor : LLVMMethod, IAccessor
    {
        public LLVMAccessor(
            LLVMProperty DeclaringProperty,
            AccessorType AccessorType,
            IMethodSignatureTemplate Template)
            : base(DeclaringProperty.ParentType, Template)
        {
            this.AccessorType = AccessorType;
            this.DeclaringProperty = DeclaringProperty;
        }

        /// <inheritdoc/>
        public AccessorType AccessorType { get; private set; }

        /// <inheritdoc/>
        public IProperty DeclaringProperty { get; private set; }
    }
}