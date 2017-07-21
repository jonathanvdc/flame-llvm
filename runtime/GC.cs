namespace __compiler_rt
{
    /// <summary>
    /// Garbage collection functionality that can be used by the compiler.
    /// </summary>
    public static class GC
    {
        private static extern void* calloc(ulong num, ulong size);

        /// <summary>
        /// Allocates a region of storage that is the given number of bytes in size.
        /// The storage is zero-initialized.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static void* Allocate(ulong size)
        {
            return calloc(1, size);
        }
    }
}
