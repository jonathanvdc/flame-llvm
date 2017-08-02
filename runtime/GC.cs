namespace __compiler_rt
{
    /// <summary>
    /// Garbage collection functionality that can be used by the compiler.
    /// </summary>
    public static unsafe class GC
    {
        private static extern void* GC_malloc(ulong size);

        /// <summary>
        /// Allocates a region of storage that is the given number of bytes in size.
        /// The storage is zero-initialized.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static void* Allocate(ulong size)
        {
            return GC_malloc(size);
        }
    }
}
