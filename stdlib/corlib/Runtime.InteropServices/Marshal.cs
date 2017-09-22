using System.Primitives.InteropServices;

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Provides facilities for interacting with unmanaged code.
    /// </summary>
    public static class Marshal
    {
        /// <summary>
        /// Allocates unmanaged memory.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        public static IntPtr AllocHGlobal(int size)
        {
            return (IntPtr)Memory.AllocHGlobal(size);
        }

        /// <summary>
        /// Deallocates unmanaged memory.
        /// </summary>
        /// <param name="ptr">The number of bytes to deallocate.</param>
        public static void FreeHGlobal(IntPtr ptr)
        {
            Memory.FreeHGlobal((void*)ptr);
        }
    }
}