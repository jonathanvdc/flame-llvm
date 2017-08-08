using System;
using System.Linq;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A pass that marks all generic definitions as internal. This allows the
    /// recompiler to delete them if it wants.
    /// </summary>
    public sealed class GenericsInternalizingPass :
        IPass<MemberSignaturePassArgument<IMember>, MemberSignaturePassResult>
    {
        private GenericsInternalizingPass() { }

        /// <summary>
        /// An instance of the generics internalizing pass.
        /// </summary>
        public static readonly GenericsInternalizingPass Instance = new GenericsInternalizingPass();

        /// <summary>
        /// The name of the generics internalizing pass.
        /// </summary>
        public const string GenericsInternalizingPassName = "internalize-generics";

        public MemberSignaturePassResult Apply(MemberSignaturePassArgument<IMember> Value)
        {
            if (Value.Member is IGenericMember)
            {
                var genericMember = (IGenericMember)Value.Member;
                if (genericMember.GetIsGeneric())
                {
                    return new MemberSignaturePassResult(
                        null,
                        Enumerable.Empty<IAttribute>(),
                        Internalize);
                }
            }
            return new MemberSignaturePassResult();
        }

        private static void Internalize(AttributeMapBuilder Attributes)
        {
            Attributes.RemoveAll(AccessAttribute.AccessAttributeType);
            Attributes.Add(new AccessAttribute(AccessModifier.Assembly));
        }
    }
}