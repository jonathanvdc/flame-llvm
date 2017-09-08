using Flame.Primitives;
using LLVMSharp;

namespace Flame.LLVM
{
    /// <summary>
    /// An attribute that specifies a member's linkage.
    /// </summary>
    public sealed class LLVMLinkageAttribute : IAttribute
    {
        public LLVMLinkageAttribute(LLVMLinkage Linkage)
        {
            this.Linkage = Linkage;
        }

        /// <summary>
        /// Gets the LLVM linkage this attribute describes.
        /// </summary>
        /// <returns>The LLVM linkage.</returns>
        public LLVMLinkage Linkage { get; private set; }

        static LLVMLinkageAttribute()
        {
            LinkageAttributeType = new PrimitiveType<IAttribute>("LinkageAttribute", 0, null);
        }

        public static readonly IType LinkageAttributeType;

        public IType AttributeType => LinkageAttributeType;

        public IBoundObject Value => new BoundPrimitive<IAttribute>(AttributeType, this);
    }
}