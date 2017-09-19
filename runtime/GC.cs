namespace __compiler_rt
{
    /// <summary>
    /// Garbage collection functionality that can be used by the compiler.
    /// </summary>
    public static unsafe class GC
    {
        [#builtin_attribute(NoAliasAttribute)]
        [#builtin_attribute(NoThrowAttribute)]
        private static extern void* GC_malloc(ulong size);

        /// <summary>
        /// Allocates a region of storage that is a number of bytes in size.
        /// The storage is zero-initialized and a pointer to it is returned.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        /// <returns>A pointer to a region of storage.</returns>
        [#builtin_attribute(NoAliasAttribute)]
        [#builtin_attribute(NoThrowAttribute)]
        public static void* Allocate(ulong size)
        {
            return GC_malloc(size);
        }
    }
}
