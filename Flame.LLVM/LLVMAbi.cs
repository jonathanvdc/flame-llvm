namespace Flame.LLVM
{
    /// <summary>
    /// Describes an LLVM ABI.
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
    }
}