using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flame.Build;
using Flame.Compiler.Build;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// A type builder for LLVM assemblies.
    /// </summary>
    public sealed class LLVMType : ITypeBuilder
    {
        public LLVMType(LLVMNamespace Namespace, ITypeSignatureTemplate Template)
        {
            this.Namespace = Namespace;
            this.templateInstance = new TypeSignatureInstance(Template, this);
            this.attrMap = new AttributeMapBuilder();
        }

        private AttributeMapBuilder attrMap;

        private TypeSignatureInstance templateInstance;

        /// <summary>
        /// Gets this LLVM type's declaring namespace.
        /// </summary>
        /// <returns>The declaring namespace.</returns>
        public LLVMNamespace Namespace { get; private set; }

        /// <summary>
        /// Gets the name of this type.
        /// </summary>
        /// <returns>This type's name.</returns>
        public UnqualifiedName Name => templateInstance.Name;

        /// <summary>
        /// Gets this type's qualified name.
        /// </summary>
        /// <returns>The type's qualified name.</returns>
        public QualifiedName FullName => Name.Qualify(Namespace.FullName);

        /// <summary>
        /// Gets this LLVM type's declaring namespace.
        /// </summary>
        /// <returns>The declaring namespace.</returns>
        public INamespace DeclaringNamespace => Namespace;

        /// <summary>
        /// Gets the ancestry rules for this type.
        /// </summary>
        public IAncestryRules AncestryRules => DefinitionAncestryRules.Instance;

        /// <summary>
        /// Gets this type's attribute map.
        /// </summary>
        /// <returns>The attribute map.</returns>
        public AttributeMap Attributes => new AttributeMap(attrMap);

        public IEnumerable<IMethod> Methods => Enumerable.Empty<IMethod>();

        public IEnumerable<IType> BaseTypes => Enumerable.Empty<IType>();

        public IEnumerable<IProperty> Properties => Enumerable.Empty<IProperty>();

        public IEnumerable<IField> Fields => Enumerable.Empty<IField>();

        public IEnumerable<IGenericParameter> GenericParameters => Enumerable.Empty<IGenericParameter>();

        public IType Build()
        {
            return this;
        }

        public IFieldBuilder DeclareField(IFieldSignatureTemplate Template)
        {
            throw new NotImplementedException();
        }

        public IMethodBuilder DeclareMethod(IMethodSignatureTemplate Template)
        {
            throw new NotImplementedException();
        }

        public IPropertyBuilder DeclareProperty(IPropertySignatureTemplate Template)
        {
            throw new NotImplementedException();
        }

        public IBoundObject GetDefaultValue()
        {
            return null;
        }

        public void Initialize()
        {
            this.attrMap.AddRange(templateInstance.Attributes.Value);

            if (templateInstance.GenericParameters.Value.Any())
            {
                throw new NotSupportedException("LLVM types do not support generic parameters");
            }

            if (templateInstance.BaseTypes.Value.Any())
            {
                throw new NotImplementedException("LLVM types do not support base types yet");
            }
        }
    }
}

