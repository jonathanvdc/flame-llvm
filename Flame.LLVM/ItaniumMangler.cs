using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Flame.LLVM
{
    /// <summary>
    /// A name mangler implementation that strives for compatibility with the
    /// Itanium C++ ABI name mangling scheme.
    /// </summary>
    public sealed class ItaniumMangler : NameMangler
    {
        private ItaniumMangler() { }

        /// <summary>
        /// An instance of a Itanium name mangler.
        /// </summary>
        public static readonly ItaniumMangler Instance = new ItaniumMangler();

        private static Dictionary<IType, string> builtinTypeNames =
            new Dictionary<IType, string>()
        {
            { PrimitiveTypes.Void, "v" },
            { PrimitiveTypes.Char, "w" },
            { PrimitiveTypes.Boolean, "b" },
            { PrimitiveTypes.Int8, "a" },
            { PrimitiveTypes.UInt8, "h" },
            { PrimitiveTypes.Int16, "s" },
            { PrimitiveTypes.UInt16, "t" },
            { PrimitiveTypes.Int32, "i" },
            { PrimitiveTypes.UInt32, "j" },
            { PrimitiveTypes.Int64, "l" },
            { PrimitiveTypes.UInt64, "m" },
            { PrimitiveTypes.Float32, "f" },
            { PrimitiveTypes.Float64, "d" }
        };

        private static Dictionary<PointerKind, string> pointerKindNames =
            new Dictionary<PointerKind, string>
        {
            { PointerKind.TransientPointer, "P" },
            { PointerKind.ReferencePointer, "R" }
        };

        /// <inheritdoc/>
        public override string Mangle(IMethod Method)
        {
            return "_Z" + EncodeFunctionName(Method);
        }

        /// <inheritdoc/>
        public override string Mangle(IField Field)
        {
            return "_Z" + EncodeQualifiedName(Field);
        }

        /// <inheritdoc/>
        public override string Mangle(IType Type, bool IncludeNamespace)
        {
            return EncodeTypeName(Type, IncludeNamespace);
        }

        private static string EncodeFunctionName(IMethod Method)
        {
            var funcName = Method.DeclaringType == null
                ? EncodeQualifiedName(Method)
                : "N" + EncodeQualifiedName(Method) + "E";
            return funcName + EncodeBareFunctionType(Method, false);
        }

        private static string EncodeBareFunctionType(
            IMethod Method, bool IncludeReturnType)
        {
            var builder = new StringBuilder();
            foreach (var param in Method.Parameters)
            {
                builder.Append(EncodeTypeName(param.ParameterType));
            }
            if (builder.Length == 0)
            {
                // Itanium ABI says:
                //
                //     Empty parameter lists, whether declared as () or conventionally
                //     as (void), are encoded with a void parameter specifier (v).
                //
                builder.Append("v");
            }
            if (IncludeReturnType)
            {
                builder.Append(EncodeTypeName(Method.ReturnType));
            }
            return builder.ToString();
        }

        private static string EncodeTypeName(IType Type)
        {
            return EncodeTypeName(Type, true);
        }

        private static string EncodeTypeName(IType Type, bool IncludeNamespace)
        {
            string result;
            if (builtinTypeNames.TryGetValue(Type, out result))
            {
                return result;
            }
            else if (Type.GetIsPointer())
            {
                var ptrType = Type.AsPointerType();
                string prefix;
                if (!pointerKindNames.TryGetValue(ptrType.PointerKind, out prefix))
                {
                    // Use a vendor-specific prefix if we can't map the pointer kind
                    // to a C++ pointer kind.
                    prefix = "U" + EncodeUnqualifiedName("P" + ptrType.PointerKind.Extension);
                }
                return prefix + EncodeTypeName(ptrType.ElementType);
            }
            else if (Type.GetIsArray())
            {
                var arrayType = Type.AsArrayType();
                return "U" + EncodeUnqualifiedName("Array" + arrayType.ArrayRank + "D" + EncodeTypeName(arrayType.ElementType));
            }
            else if (Type.GetIsGenericInstance())
            {
                var builder = new StringBuilder();
                builder.Append(EncodeTypeName(Type.GetGenericDeclaration(), IncludeNamespace));
                builder.Append("I");
                foreach (var arg in Type.GetGenericArguments())
                {
                    builder.Append(EncodeTypeName(arg));
                }
                builder.Append("E");
                return builder.ToString();
            }
            else if (IncludeNamespace)
            {
                return EncodeQualifiedName(Type);
            }
            else
            {
                return EncodeUnqualifiedName(Type.Name);
            }
        }

        private static string EncodeQualifiedName(IMember Member)
        {
            if (Member is INamespace && !(Member is IType))
            {
                return EncodeNamespaceName((INamespace)Member);
            }

            var suffix = EncodeUnqualifiedName(Member.Name);
            var declaringMember = GetDeclaringMember(Member);
            if (declaringMember == null)
            {
                return suffix;
            }
            else if (declaringMember is IType)
            {
                return EncodeTypeName((IType)declaringMember) + suffix;
            }
            else
            {
                return EncodeQualifiedName(declaringMember) + suffix;
            }
        }

        private static string EncodeNamespaceName(INamespace Namespace)
        {
            return EncodeNamespaceName(Namespace.FullName);
        }

        private static string EncodeNamespaceName(QualifiedName Name)
        {
            var result = new StringBuilder();
            while (!Name.IsEmpty)
            {
                result.Append(EncodeUnqualifiedName(Name.Qualifier.ToString()));
                Name = Name.Name;
            }
            return result.ToString();
        }

        private static string EncodeUnqualifiedName(string Name)
        {
            var utf8Length = UTF8Encoding.UTF8.GetByteCount(Name);
            if (utf8Length == 0)
                return Name;
            else
                return utf8Length.ToString(CultureInfo.InvariantCulture) + Name;
        }

        private static string EncodeUnqualifiedName(UnqualifiedName Name)
        {
            if (Name is SimpleName)
            {
                return EncodeUnqualifiedName(((SimpleName)Name).Name);
            }
            else if (Name is PreMangledName)
            {
                return ((PreMangledName)Name).Name;
            }
            else
            {
                return EncodeUnqualifiedName(Name.ToString());
            }
        }

        private static IMember GetDeclaringMember(IMember Member)
        {
            if (Member is ITypeMember)
            {
                var declType = ((ITypeMember)Member).DeclaringType;
                if (declType != null)
                    return declType;
            }
            if (Member is IType)
            {
                var declNs = ((IType)Member).DeclaringNamespace;
                if (declNs != null)
                    return declNs;
            }
            return null;
        }
    }
}