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
            this.RelativeVTable = new VTableSlots();
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
        /// Gets this type's vtable slots for methods that are declared in this type.
        /// </summary>
        /// <returns>The vtable slots.</returns>
        public VTableSlots RelativeVTable { get; private set; }

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
        public static readonly LLVMTypeRef VTableType = StructType(
            new LLVMTypeRef[]
            {
                Int64Type(),
                ArrayType(PointerType(Int8Type(), 0), 0)
            },
            false);

        /// <summary>
        /// Defines the vtable for this type.
        /// </summary>
        /// <param name="Module">The module to define the vtable in.</param>
        /// <returns>An LLVM global for this type's vtable.</returns>
        public VTableInstance DefineVTable(LLVMModuleBuilder Module)
        {
            var allEntries = new List<LLVMMethod>();
            GetAllVTableEntries(allEntries);

            var fields = new LLVMValueRef[2];
            fields[0] = ConstInt(Int64Type(), Module.GetTypeId(this), false);
            fields[1] = ConstArray(PointerType(Int8Type(), 0), GetVTableEntryImpls(Module, allEntries));
            var vtableContents = ConstStruct(fields, false);
            var vtable = Module.DeclareGlobal(
                VTableType,
                FullName.ToString() + ".vtable");
            vtable.SetGlobalConstant(true);
            vtable.SetLinkage(LLVMLinkage.LLVMInternalLinkage);
            vtable.SetInitializer(vtableContents);
            return new VTableInstance(vtable, allEntries);
        }

        private void GetAllVTableEntries(List<LLVMMethod> Results)
        {
            var parent = this.GetParent() as LLVMType;
            if (parent != null)
            {
                parent.GetAllVTableEntries(Results);
            }
            Results.AddRange(RelativeVTable.Entries);
        }

        private LLVMValueRef[] GetVTableEntryImpls(
            LLVMModuleBuilder Module,
            List<LLVMMethod> AllEntries)
        {
            var allImpls = new LLVMValueRef[AllEntries.Count];
            for (int i = 0; i < allImpls.Length; i++)
            {
                allImpls[i] = ConstBitCast(
                    Module.Declare(AllEntries[i].GetImplementation(this)),
                    PointerType(Int8Type(), 0));
            }
            return allImpls;
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

    /// <summary>
    /// A data structure that manages a type's virtual function table slots.
    /// </summary>
    public sealed class VTableSlots
    {
        public VTableSlots()
        {
            this.slots = new Dictionary<LLVMMethod, int>();
            this.contents = new List<LLVMMethod>();
        }

        private Dictionary<LLVMMethod, int> slots;
        private List<LLVMMethod> contents;

        /// <summary>
        /// Gets the entries in this vtable.
        /// </summary>
        /// <returns>The vtable entries.</returns>
        public IReadOnlyList<LLVMMethod> Entries => contents;

        /// <summary>
        /// Creates a relative vtable slot for the given method.
        /// </summary>
        /// <param name="Method">The method to create a vtable slot for.</param>
        /// <returns>A vtable slot.</returns>
        public int CreateRelativeSlot(LLVMMethod Method)
        {
            int slot = contents.Count;
            contents.Add(Method);
            slots.Add(Method, slot);
            return slot;
        }

        /// <summary>
        /// Gets the relative vtable slot for the given method.
        /// </summary>
        /// <param name="Method">The method to get a vtable slot for.</param>
        /// <returns>A vtable slot.</returns>
        public int GetRelativeSlot(LLVMMethod Method)
        {
            return slots[Method];
        }
    }

    /// <summary>
    /// Describes a vtable instance: a concrete vtable for a specific class.
    /// </summary>
    public sealed class VTableInstance
    {
        public VTableInstance(LLVMValueRef Pointer, IReadOnlyList<LLVMMethod> Entries)
        {
            this.Pointer = Pointer;
            this.absSlots = new Dictionary<LLVMMethod, int>();
            for (int i = 0; i < Entries.Count; i++)
            {
                this.absSlots[Entries[i]] = i;
            }
        }

        /// <summary>
        /// Gets a pointer to the vtable.
        /// </summary>
        /// <returns>A pointer to the vtable.</returns>
        public LLVMValueRef Pointer { get; private set; }

        private Dictionary<LLVMMethod, int> absSlots;

        /// <summary>
        /// Gets the absolute vtable slot for the given method.
        /// </summary>
        /// <param name="Method">The method to get the absolute vtable slot for.</param>
        /// <returns>An absolute vtable slot.</returns>
        public int GetAbsoluteSlot(LLVMMethod Method)
        {
            int slot;
            if (!absSlots.TryGetValue(Method, out slot))
            {
                slot = GetAbsoluteSlot(Method.ParentMethod);
                absSlots[Method] = slot;
            }
            return slot;
        }
    }
}

