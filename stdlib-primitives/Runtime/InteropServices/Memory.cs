namespace System.Primitives.InteropServices
{
    /// <summary>
    /// Handles unmanaged memory.
    /// </summary>
    public static class Memory
    {
        private static extern void* calloc(size_t num, size_t size);
        private static extern void free(void* ptr);
        private static extern void* memcpy(void* dest, void* src, size_t count);
        private static extern void* memmove(void* dest, void* src, size_t count);

        /// <summary>
        /// Allocates unmanaged memory.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static void* AllocHGlobal(ulong size)
        {
            return calloc((size_t)1u, (size_t)size);
        }

        /// <summary>
        /// Deallocates unmanaged memory.
        /// </summary>
        /// <param name="ptr">The number of bytes to deallocate.</param>
        public static void FreeHGlobal(void* ptr)
        {
            free(ptr);
        }

        /// <summary>
        /// Copies a number of bytes from one address in memory to another,
        /// where the buffers are known not to overlap.
        /// </summary>
        /// <param name="source">
        /// The address of the source buffer.
        /// </param>
        /// <param name="destination">
        /// The address of the destination buffer.
        /// </param>
        /// <param name="count">
        /// The number of bytes to copy from the source buffer.
        /// </param>
        public static void MemoryCopy(void* source, void* destination, ulong count)
        {
            memcpy(destination, source, (size_t)count);
        }

        /// <summary>
        /// Copies a number of bytes from one address in memory to another.
        /// The buffers may overlap.
        /// </summary>
        /// <param name="source">
        /// The address of the source buffer.
        /// </param>
        /// <param name="destination">
        /// The address of the destination buffer.
        /// </param>
        /// <param name="count">
        /// The number of bytes to copy from the source buffer.
        /// </param>
        public static void MemoryMove(void* source, void* destination, ulong count)
        {
            memmove(destination, source, (size_t)count);
        }
    }
}