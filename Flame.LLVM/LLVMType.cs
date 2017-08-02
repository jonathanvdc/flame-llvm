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
            this.fieldCounter = 0;
            this.declaredProperties = new List<LLVMProperty>();
        }

        private AttributeMapBuilder attrMap;

        private TypeSignatureInstance templateInstance;

        private List<LLVMMethod> declaredMethods;
        private List<LLVMField> declaredInstanceFields;
        private int fieldCounter;
        private List<LLVMField> declaredStaticFields;
        private List<LLVMField> declaredFields;
        private List<LLVMProperty> declaredProperties;

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

        /// <summary>
        /// Tests if this type is a value type that is stored as a single value.
        /// The runtime representation of these types is not wrapped in a struct.
        /// </summary>
        /// <returns><c>true</c> if this is a single-value value type; otherwise, <c>false</c>.</returns>
        public bool IsSingleValue => InstanceFields.Count == 1 && this.GetIsValueType();

        public IEnumerable<IMethod> Methods => declaredMethods;

        public IEnumerable<IType> BaseTypes => templateInstance.BaseTypes.Value;

        public IEnumerable<IProperty> Properties => declaredProperties;

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
                Template.IsStatic ? -1 : fieldCounter);

            if (fieldDef.IsStatic)
            {
                declaredStaticFields.Add(fieldDef);
            }
            else
            {
                declaredInstanceFields.Add(fieldDef);
                fieldCounter++;
            }

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
            var propDef = new LLVMProperty(this, Template);
            declaredProperties.Add(propDef);
            return propDef;
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

            this.fieldCounter += this.GetIsValueType() ? 0 : 1;
        }

        /// <summary>
        /// Defines the data layout of this type as an LLVM type.
        /// </summary>
        /// <param name="Module">The module to define the type in.</param>
        /// <returns>An LLVM type ref for this type's data layout.</returns>
        public LLVMTypeRef DefineLayout(LLVMModuleBuilder Module)
        {
            bool isStruct = this.GetIsValueType();

            if (isStruct && IsSingleValue)
            {
                return Module.Declare(declaredInstanceFields[0].FieldType);
            }

            int offset = isStruct ? 0 : 1;
            var elementTypes = new LLVMTypeRef[offset + declaredInstanceFields.Count];
            if (!isStruct)
            {
                var baseType = this.GetParent();
                if (baseType == null)
                {
                    // Type is a root type. Embed a pointer to its vtable.
                    elementTypes[0] = Module.Declare(PrimitiveTypes.UInt8.MakePointerType(PointerKind.TransientPointer));
                }
                else
                {
                    // Type is not a root type. Embed its base type.
                    elementTypes[0] = Module.DeclareDataLayout((LLVMType)baseType);
                }
            }

            for (int i = 0; i < elementTypes.Length - offset; i++)
            {
                elementTypes[i + offset] = Module.Declare(declaredInstanceFields[i].FieldType);
            }
            return StructType(elementTypes, false);
        }

        /// <summary>
        /// The type of a vtable.
        /// </summary>
        public static readonly LLVMTypeRef VTableType = StructType(new LLVMTypeRef[] { Int64Type() }, false);

        /// <summary>
        /// Defines the vtable for this type.
        /// </summary>
        /// <param name="Module">The module to define the vtable in.</param>
        /// <returns>An LLVM global for this type's vtable.</returns>
        public LLVMValueRef DefineVTable(LLVMModuleBuilder Module)
        {
            var fields = new LLVMValueRef[1];
            fields[0] = ConstInt(Int64Type(), Module.GetTypeId(this), false);
            var vtableContents = ConstStruct(fields, false);
            var vtable = Module.DeclareGlobal(
                VTableType,
                FullName.ToString() + ".vtable");
            vtable.SetGlobalConstant(true);
            vtable.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
            vtable.SetInitializer(vtableContents);
            return vtable;
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
            foreach (var property in declaredProperties)
            {
                property.Emit(Module);
            }
        }

        public override string ToString()
        {
            return FullName.ToString();
        }
    }
}

