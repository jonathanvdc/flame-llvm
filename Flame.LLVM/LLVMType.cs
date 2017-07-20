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
            this.declaredMethods = new List<LLVMMethod>();
            this.declaredInstanceFields = new List<LLVMField>();
            this.declaredStaticFields = new List<LLVMField>();
            this.declaredFields = new List<LLVMField>();
        }

        private AttributeMapBuilder attrMap;

        private TypeSignatureInstance templateInstance;

        private List<LLVMMethod> declaredMethods;
        private List<LLVMField> declaredInstanceFields;
        private List<LLVMField> declaredStaticFields;
        private List<LLVMField> declaredFields;

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

        /// <summary>
        /// Gets the list of all instance fields defined by this type.
        /// </summary>
        public IReadOnlyList<LLVMField> InstanceFields => declaredInstanceFields;

        public IEnumerable<IMethod> Methods => declaredMethods;

        public IEnumerable<IType> BaseTypes => Enumerable.Empty<IType>();

        public IEnumerable<IProperty> Properties => Enumerable.Empty<IProperty>();

        public IEnumerable<IField> Fields => declaredFields;

        public IEnumerable<IGenericParameter> GenericParameters => Enumerable.Empty<IGenericParameter>();

        public IType Build()
        {
            return this;
        }

        public IFieldBuilder DeclareField(IFieldSignatureTemplate Template)
        {
            var fieldDef = new LLVMField(
                this,
                Template,
                Template.IsStatic ? -1 : declaredInstanceFields.Count);

            if (fieldDef.IsStatic)
                declaredStaticFields.Add(fieldDef);
            else
                declaredInstanceFields.Add(fieldDef);

            declaredFields.Add(fieldDef);
            return fieldDef;
        }

        public IMethodBuilder DeclareMethod(IMethodSignatureTemplate Template)
        {
            var methodDef = new LLVMMethod(this, Template);
            declaredMethods.Add(methodDef);
            return methodDef;
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

            if (templateInstance.GenericParameters.Value.Any<IType>())
            {
                throw new NotSupportedException("LLVM types do not support generic parameters");
            }

            if (templateInstance.BaseTypes.Value.Any<IType>())
            {
                throw new NotImplementedException("LLVM types do not support base types yet");
            }
        }

        /// <summary>
        /// Defines the data layout of this type as an LLVM type.
        /// </summary>
        /// <param name="Module">The module to define the type in.</param>
        /// <returns>An LLVM type ref for this type's data layout.</returns>
        public LLVMTypeRef DefineLayout(LLVMModuleBuilder Module)
        {
            var elementTypes = new LLVMTypeRef[declaredInstanceFields.Count];
            for (int i = 0; i < elementTypes.Length; i++)
            {
                elementTypes[i] = Module.Declare(declaredInstanceFields[i].FieldType);
            }
            return StructType(elementTypes, false);
        }

        /// <summary>
        /// Writes this type's definitions to the given module.
        /// </summary>
        /// <param name="Module">The module to populate.</param>
        public void Emit(LLVMModuleBuilder Module)
        {
            foreach (var method in declaredMethods)
            {
                method.Emit(Module);
            }
        }

        public override string ToString()
        {
            return FullName.ToString();
        }
    }
}

