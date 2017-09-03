using System;

namespace Flame.LLVM
{
    /// <summary>
    /// An ABI description for an LLVM back-end configuration.
    /// </summary>
    public sealed class LLVMAbi
    {
        /// <summary>
        /// Creates an LLVM ABI.
        /// </summary>
        public LLVMAbi(
            NameMangler Mangler,
            GCDescription GarbageCollector,
            EHDescription ExceptionHandling)
        {
            this.Mangler = Mangler;
            this.GarbageCollector = GarbageCollector;
            this.ExceptionHandling = ExceptionHandling;
        }

        /// <summary>
        /// Gets the name mangler for this ABI.
        /// </summary>
        /// <returns>The name mangler.</returns>
        public NameMangler Mangler { get; private set; }

        /// <summary>
        /// Gets the garbage collector description for this ABI.
        /// </summary>
        /// <returns>The garbage collector interface.</returns>
        public GCDescription GarbageCollector { get; private set; }

        /// <summary>
        /// Gets the exception handling description for this ABI.
        /// </summary>
        /// <returns>The exception handling description.</returns>
        public EHDescription ExceptionHandling { get; private set; }

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