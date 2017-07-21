using System;
using System.Collections.Generic;
using Flame.Compiler.Build;

namespace Flame.LLVM
{
    /// <summary>
    /// An property implementation for the LLVM back-end.
    /// </summary>
    public sealed class LLVMProperty : IPropertyBuilder
    {
        public LLVMProperty(LLVMType ParentType, IPropertySignatureTemplate Template)
        {
            this.ParentType = ParentType;
            this.templateInstance = new PropertySignatureInstance(Template, this);
            this.declaredAccessors = new List<LLVMAccessor>();
        }

        /// <summary>
        /// Gets this property's declaring type as an LLVM type.
        /// </summary>
        /// <returns>The declaring type.</returns>
        public LLVMType ParentType { get; private set; }

        private PropertySignatureInstance templateInstance;

        private List<LLVMAccessor> declaredAccessors;

        public IEnumerable<IParameter> IndexerParameters => templateInstance.IndexerParameters.Value;

        public IType PropertyType => templateInstance.PropertyType.Value;

        public IEnumerable<IAccessor> Accessors => declaredAccessors;

        public bool IsStatic => templateInstance.Template.IsStatic;

        public IType DeclaringType => ParentType;

        public AttributeMap Attributes => templateInstance.Attributes.Value;

        public UnqualifiedName Name => templateInstance.Name;

        public QualifiedName FullName => Name.Qualify(DeclaringType.FullName);

        public IMethodBuilder DeclareAccessor(AccessorType Type, IMethodSignatureTemplate Template)
        {
            var result = new LLVMAccessor(this, Type, Template);
            declaredAccessors.Add(result);
            return result;
        }

        public void Initialize()
        {
        }

        public IProperty Build()
        {
            return this;
        }

        /// <summary>
        /// Writes this type's definitions to the given module.
        /// </summary>
        /// <param name="Module">The module to populate.</param>
        public void Emit(LLVMModuleBuilder Module)
        {
            foreach (var accessors in declaredAccessors)
            {
                accessors.Emit(Module);
            }
        }
    }
}