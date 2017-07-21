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
                return "UArray" + arrayType.ArrayRank + "D" + EncodeTypeName(arrayType.ElementType);
            }
            else
            {
                return EncodeQualifiedName(Type);
            }
        }

        private static string EncodeQualifiedName(IMember Member)
        {
            var suffix = EncodeUnqualifiedName(Member.Name.ToString());
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

        private static string EncodeUnqualifiedName(string Name)
        {
            var utf8Length = UTF8Encoding.UTF8.GetByteCount(Name);
            if (utf8Length == 0)
                return Name;
            else
                return utf8Length.ToString(CultureInfo.InvariantCulture) + Name;
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