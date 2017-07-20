using System;
using Flame.Compiler;
using Flame.Compiler.Build;

namespace Flame.LLVM
{
    /// <summary>
    /// A field build implementation for LLVM fields.
    /// </summary>
    public sealed class LLVMField : IFieldBuilder
    {
        public LLVMField(LLVMType DeclaringType, IFieldSignatureTemplate Template, int FieldIndex)
        {
            this.DeclaringType = DeclaringType;
            this.templateInstance = new FieldSignatureInstance(Template, this);
            this.FieldIndex = FieldIndex;
        }

        /// <inheritdoc/>
        public IType DeclaringType { get; private set; }

        /// <summary>
        /// Gets the index of this field in the type that defines it.
        /// </summary>
        /// <returns>The field index of this field.</returns>
        public int FieldIndex { get; private set; }

        private FieldSignatureInstance templateInstance;

        /// <inheritdoc/>
        public IType FieldType => templateInstance.FieldType.Value;

        /// <inheritdoc/>
        public bool IsStatic => templateInstance.Template.IsStatic;

        /// <inheritdoc/>
        public AttributeMap Attributes => templateInstance.Attributes.Value;

        /// <inheritdoc/>
        public UnqualifiedName Name => templateInstance.Name;

        /// <inheritdoc/>
        public QualifiedName FullName => Name.Qualify(DeclaringType.FullName);

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