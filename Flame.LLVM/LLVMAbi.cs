using System;

namespace Flame.LLVM
{
    /// <summary>
    /// An ABI description for an LLVM back-end configuration.
    /// </summary>
    public sealed class LLVMAbi
    {
        /// <summary>
        /// Creates an LLVM ABI from the given name mangler.
        /// </summary>
        public LLVMAbi(NameMangler Mangler)
        {
            this.Mangler = Mangler;
        }

        /// <summary>
        /// Gets the name mangler for this ABI.
        /// </summary>
        /// <returns>The name mangler.</returns>
        public NameMangler Mangler { get; private set; }

        private LLVMAbi GetThis()
        {
            return this;
        }

        /// <summary>
        /// Wraps this LLVM ABI in a lazy object.
        /// </summary>
        /// <returns>A lazy wrapper for this ABI.</returns>
        public Lazy<LLVMAbi> AsLazyAbi()
        {
            return new Lazy<LLVMAbi>(GetThis);
        }
    }
}