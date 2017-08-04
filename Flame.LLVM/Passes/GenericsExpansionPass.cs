using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A pass that expands references to generic types and members to non-generic equivalents.
    /// </summary>
    public sealed class GenericsExpansionPass : IPass<BodyPassArgument, IStatement>
    {
        public GenericsExpansionPass(Func<IType, UnqualifiedName> ExpandedTypeNamer)
        {
            this.ExpandedTypeNamer = ExpandedTypeNamer;
        }

        /// <summary>
        /// Gets the function that takes a generic instance and produces a name for the
        /// expanded type that is equivalent to that generic instance.
        /// </summary>
        /// <returns>A function that maps types to names.</returns>
        public Func<IType, UnqualifiedName> ExpandedTypeNamer { get; private set; }

        /// <summary>
        /// Gets the name of the generics expansion pass.
        /// </summary>
        public const string GenericsExpansionPassName = "expand-generics";

        /// <inheritdoc/>
        public IStatement Apply(BodyPassArgument Argument)
        {
            if (Argument.DeclaringMethod.GetIsRecursiveGenericInstance())
            {
                return Argument.Body;
            }

            string genericsExpanderOption = "generics-expander";
            var expander = Argument.Metadata.GlobalMetadata.GetOption<GenericsExpander>(
                genericsExpanderOption,
                null);

            if (expander == null)
            {
                expander = new GenericsExpander(Argument.PassEnvironment, ExpandedTypeNamer);
                Argument.Metadata.GlobalMetadata.SetOption<GenericsExpander>(
                    genericsExpanderOption,
                    expander);
            }

            return MemberNodeVisitor.ConvertMembers(expander.MemberConverter, Argument.Body);
        }
    }

    /// <summary>
    /// A type of object that expands generic instances.
    /// </summary>
    public sealed class GenericsExpander : TypeTransformerBase
    {
        public GenericsExpander(
            IBodyPassEnvironment PassEnvironment,
            Func<IType, UnqualifiedName> ExpandedTypeNamer)
        {
            this.PassEnvironment = PassEnvironment;
            this.ExpandedTypeNamer = ExpandedTypeNamer;
            this.expandedTypes = new Dictionary<IType, IType>();
            this.expandedMethods = new Dictionary<IMethod, IMethod>();
            this.expandedFields = new Dictionary<IField, IField>();
        }

        /// <summary>
        /// Gets the pass environment used by this generics expander.
        /// </summary>
        /// <returns>The pass environment.</returns>
        public IBodyPassEnvironment PassEnvironment { get; private set; }

        /// <summary>
        /// Gets the function that takes a generic instance and produces a name for the
        /// expanded type that is equivalent to that generic instance.
        /// </summary>
        /// <returns>A function that maps types to names.</returns>
        public Func<IType, UnqualifiedName> ExpandedTypeNamer { get; private set; }

        private Dictionary<IType, IType> expandedTypes;
        private Dictionary<IMethod, IMethod> expandedMethods;
        private Dictionary<IField, IField> expandedFields;

        /// <summary>
        /// Gets a member converter for this expander.
        /// </summary>
        /// <returns>A member converter.</returns>
        public MemberConverter MemberConverter
        {
            get
            {
                return new MemberConverter(
                    new DelegateConverter<IType, IType>(Convert),
                    new DelegateConverter<IMethod, IMethod>(Convert),
                    new DelegateConverter<IField, IField>(Convert));
            }
        }

        protected override IType ConvertGenericInstance(IType Type)
        {
            if (!Type.GetIsRecursiveGenericInstance())
            {
                return Type;
            }

            IType result;
            if (!expandedTypes.TryGetValue(Type, out result))
            {
                result = ExpandTypeImpl(Type);
            }
            return result;
        }

        private IType ExpandTypeImpl(IType Type)
        {
            if (!IsCompleteGenericInstance(Type))
            {
                expandedTypes[Type] = Type;
                return Type;
            }

            var declType = Type.DeclaringNamespace as IType;
            var expandedDeclNs = declType == null
                ? Type.DeclaringNamespace
                : (INamespace)Convert(declType);

            if (expandedTypes.ContainsKey(Type))
            {
                // Converting the declaring type already triggered an expansion
                // for this type.
                return expandedTypes[Type];
            }

            // Define a new type.
            var expandedType = new DescribedType(ExpandedTypeNamer(Type), expandedDeclNs);
            expandedTypes[Type] = expandedType;

            // Copy the attributes.
            foreach (var attr in Type.Attributes)
            {
                expandedType.AddAttribute(attr);
            }

            // Convert the type's base types.
            foreach (var baseType in Type.BaseTypes)
            {
                expandedType.AddBaseType(Convert(baseType));
            }

            // Convert the type's fields.
            foreach (var field in Type.Fields)
            {
                expandedType.AddField(ExpandFieldImpl(expandedType, field));
            }

            return expandedType;
        }

        public IMethod Convert(IMethod Method)
        {
            if (!Method.GetIsRecursiveGenericInstance())
            {
                return Method;
            }

            IMethod result;
            if (!expandedMethods.TryGetValue(Method, out result))
            {
                result = ExpandMethodImpl(Method);
            }
            return result;
        }

        private IMethod ExpandMethodImpl(IMethod Method)
        {
            if (!IsCompleteGenericInstance(Method))
            {
                expandedMethods[Method] = Method;
                return Method;
            }

            if (!PassEnvironment.CanExtend(Method.DeclaringType))
            {
                return Method;
            }

            var declaringType = Convert(Method.DeclaringType);

            if (expandedMethods.ContainsKey(Method))
            {
                // Converting the declaring type already triggered an expansion
                // for this method.
                return expandedMethods[Method];
            }

            // Declare a new method.
            var expandedMethod = new DescribedBodyMethod(
                Method.Name,
                declaringType);

            expandedMethod.IsStatic = Method.IsStatic;

            expandedMethods[Method] = expandedMethod;

            // Copy the attributes.
            foreach (var attr in Method.Attributes)
            {
                expandedMethod.AddAttribute(attr);
            }

            // Convert the method's return type.
            expandedMethod.ReturnType = Convert(Method.ReturnType);

            // Convert the method's parameters.
            foreach (var param in Method.Parameters)
            {
                expandedMethod.AddParameter(
                    new RetypedParameter(
                        param,
                        Convert(param.ParameterType)));
            }

            // Convert the method's base methods.
            foreach (var baseMethod in Method.BaseMethods)
            {
                expandedMethod.AddBaseMethod(Convert(baseMethod));
            }

            // Convert the method's body.
            expandedMethod.Body = PassEnvironment.GetMethodBody(Method);

            return expandedMethod;
        }

        public IField Convert(IField Field)
        {
            Convert(Field.DeclaringType);
            IField result;
            if (expandedFields.TryGetValue(Field, out result))
            {
                return result;
            }
            else
            {
                return Field;
            }
        }

        private IField ExpandFieldImpl(IType DeclaringType, IField Field)
        {
            var newField = new DescribedField(
                Field.Name,
                DeclaringType,
                PrimitiveTypes.Void,
                Field.IsStatic);

            expandedFields[Field] = newField;

            // Convert the field type.
            newField.FieldType = Convert(Field.FieldType);

            // Copy the attributes.
            foreach (var attr in Field.Attributes)
            {
                newField.AddAttribute(attr);
            }

            // TODO: convert the field's initial value.

            return newField;
        }

        private static bool IsCompleteGenericInstance(
            IEnumerable<IGenericParameter> RecursiveGenericParameters,
            IEnumerable<IType> RecursiveGenericArguments)
        {
            var recGenericParams = RecursiveGenericParameters
                .ToArray<IGenericParameter>();

            var recGenericArgs = RecursiveGenericArguments
                .ToArray<IType>();

            if (recGenericParams.Length != recGenericArgs.Length)
            {
                return false;
            }

            foreach (var item in recGenericArgs)
            {
                if (!IsCompleteGenericInstance(item))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsCompleteGenericInstance(IType Type)
        {
            return !Type.GetIsGenericParameter()
                && IsCompleteGenericInstance(
                    Type.GetRecursiveGenericParameters(),
                    Type.GetRecursiveGenericArguments());
        }

        private static bool IsCompleteGenericInstance(IMethod Method)
        {
            return IsCompleteGenericInstance(
                Method.GetRecursiveGenericParameters(),
                Method.GetRecursiveGenericArguments());
        }

        private static bool IsCompleteGenericInstance(IField Field)
        {
            return IsCompleteGenericInstance(Field.DeclaringType);
        }
    }
}