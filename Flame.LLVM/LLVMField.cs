using System;
using Flame.Compiler;
using Flame.Compiler.Build;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// A field build implementation for LLVM fields.
    /// </summary>
    public sealed class LLVMField : LLVMSymbolTypeMember, IFieldBuilder
    {
        public LLVMField(LLVMType DeclaringType, IFieldSignatureTemplate Template, int FieldIndex)
            : base(DeclaringType)
        {
            this.templateInstance = new FieldSignatureInstance(Template, this);
            this.FieldIndex = FieldIndex;
        }

        /// <summary>
        /// Gets the index of this field in the type that defines it.
        /// </summary>
        /// <returns>The field index of this field.</returns>
        public int FieldIndex { get; private set; }

        /// <summary>
        /// Tells if this field is the value of a single-value type.
        /// </summary>
        public bool IsSingleValueField => ParentType.IsSingleValue && !IsStatic;

        private FieldSignatureInstance templateInstance;

        /// <inheritdoc/>
        public IType FieldType => templateInstance.FieldType.Value;

        /// <inheritdoc/>
        public override bool IsStatic => templateInstance.Template.IsStatic;

        /// <inheritdoc/>
        public override AttributeMap Attributes => templateInstance.Attributes.Value;

        /// <inheritdoc/>
        public override UnqualifiedName Name => templateInstance.Name;

        public IField Build()
        {
            return this;
        }

        public void Initialize()
        {
        }

        public bool TrySetValue(IExpression Value)
        {
            return false;
        }
    }
}