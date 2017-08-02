namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Provides facilities for interacting with unmanaged code.
    /// </summary>
    public static class Marshal
    {
        private static extern void* calloc(ulong num, ulong size);
        private static extern void free(void* ptr);

        /// <summary>
        /// Allocates unmanaged memory.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static void* AllocHGlobal(int size)
        {
            return calloc(1, (ulong)size);
        }

        /// <summary>
        /// Allocates unmanaged memory.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static void* AllocHGlobal(ulong size)
        {
            return calloc(1, size);
        }

        /// <summary>
        /// Deallocates unmanaged memory.
        /// </summary>
        /// <param name="ptr">The number of bytes to deallocate.</param>
        public static void FreeHGlobal(void* ptr)
        {
            free(ptr);
        }
    }
}