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

        /// <summary>
        /// Copies all characters up to the first null character from an unmanaged UTF-8 string
        /// to a UTF-16 string.
        /// </summary>
        /// <param name="ptr">The pointer to the UTF-8 string.</param>
        /// <returns>A UTF-16 string if <c>ptr</c> is not <c>IntPtr.Zero</c>; otherwise, <c>null</c>.</returns>
        public static string PtrToStringAnsi(IntPtr ptr)
        {
            if (ptr.ToPointer() == (void*)null)
                return null;
            else
                return String.FromCString((byte*)ptr.ToPointer());
        }

        /// <summary>
        /// Allocates an unmanaged buffer and fills it with this string's contents,
        /// re-encoded as UTF-8. The resulting buffer is terminated by the null
        /// terminator character. The caller is responsible for freeing the buffer
        /// when it's done using it.
        /// </summary>
        /// <param name="str">The UTF-16 string to convert to a UTF-8 string.</param>
        /// <returns>A C-style string for which the caller is responsible.</returns>
        public static IntPtr StringToHGlobalAnsi(string str)
        {
            if (str == null)
                return new IntPtr((void*)null);
            else
                return new IntPtr(String.ToCString(str));
        }

        /// <summary>
        /// Converts a delegate of a specified type to a function pointer that is callable from unmanaged code.
        /// </summary>
        /// <param name="value">The delegate to convert to a function pointer.</param>
        /// <returns>A function pointer.</returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static void* LoadDelegateFunctionPointerInternal(object value);

        /// <summary>
        /// Tells if the given delegate has a context value.
        /// </summary>
        /// <param name="value">A delegate value.</param>
        /// <returns><c>true</c> if the delegate has a context value; otherwise, <c>false</c>.</returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static bool LoadDelegateHasContextInternal(object value);

        /// <summary>
        /// Converts a delegate of a specified type to a function pointer that is callable from unmanaged code.
        /// </summary>
        /// <param name="value">The delegate to convert to a function pointer.</param>
        /// <returns>A function pointer.</returns>
        public static IntPtr GetFunctionPointerForDelegate<TDelegate>(TDelegate value)
        {
            // if (value == null)
            // {
            //     throw new ArgumentNullException("value");
            // }
            //
            // if (LoadDelegateHasContextInternal(value))
            // {
            //     throw new NotImplementedException(
            //         "Converting a delegate with a context value to a function pointer is not implemented yet.");
            // }

            return (IntPtr)LoadDelegateFunctionPointerInternal(value);
        }
    }
}