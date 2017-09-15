using Flame.Attributes;
using Flame.Compiler.Expressions;

namespace Flame.LLVM
{
    /// <summary>
    /// Specifies LLVM intrinsic attributes.
    /// </summary>
    public static class LLVMAttributes
    {
        /// <summary>
        /// The name of ABI-specifying intrinsic attributes for the LLVM back-end.
        /// </summary>
        public const string AbiAttributeName = "LLVMAbiAttribute";

        /// <summary>
        /// The type of an ABI-specifying intrinsic attribute.
        /// </summary>
        public static readonly IType AbiAttributeType =
            new IntrinsicAttribute(AbiAttributeName).AttributeType;

        /// <summary>
        /// Creates an attribute that specifies an ABI.
        /// </summary>
        /// <param name="AbiName">The name of the ABI.</param>
        /// <returns>An attribute that specifies an ABI.</returns>
        public static IntrinsicAttribute CreateAbiAttribute(string AbiName)
        {
            return new IntrinsicAttribute(
                AbiAttributeName,
                new IBoundObject[] { new StringExpression(AbiName) });
        }

        /// <summary>
        /// Gets the ABI name specified for the given member.
        /// </summary>
        /// <param name="Member">A member.</param>
        /// <returns>An ABI name, if one has been specified; otherwise, <c>null</c>.</returns>
        public static string GetAbiName(IMember Member)
        {
            var attr = Member.GetAttribute(AbiAttributeType) as IntrinsicAttribute;
            if (attr == null)
            {
                return null;
            }
            else
            {
                return attr.Arguments[0].GetValue<string>();
            }
        }
    }
}