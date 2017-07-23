using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// A character string that uses the UTF-16 encoding.
    /// </summary>
    public sealed class String
    {
        internal String()
        { }

        /// <summary>
        /// Creates a string from the given sequence of characters.
        /// </summary>
        /// <param name="characters">The characters the string is composed of.</param>
        public String(char[] characters)
        {
            // Copy the array to an array of our own.
            data = new char[characters.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = characters[i];
            }
        }

        internal char[] data;

        /// <summary>
        /// Gets this string's length.
        /// </summary>
        public int Length => data.Length;

        /// <summary>
        /// Gets the character at the given position in this string.
        /// </summary>
        public char this[int i] => data[i];

        /// <summary>
        /// Creates a string from the given null-terminated string
        /// of UTF-8 encoded characters.
        /// </summary>
        /// <param name="buffer">The buffer that contains the string.</param>
        /// <returns>A string.</returns>
        public static String FromCString(byte* buffer)
        {
            // TODO: actually implement proper UTF-8 -> UTF-16 conversion.
            // This naive algorithm only works for some characters.
            // (fortunately, these characters include the ASCII range)

            var str = new String();
            int length = (int)strlen(buffer);
            str.data = new char[length];
            for (int i = 0; i < length; i++)
            {
                str.data[i] = (char)buffer[i];
            }
            return str;
        }

        /// <summary>
        /// Allocates an unmanaged buffer and fills it with this string's contents,
        /// re-encoded as UTF-8. The resulting buffer is terminated by the null
        /// terminator character. The caller is responsible for freeing the buffer
        /// when it's done using it.
        /// </summary>
        /// <param name="str">The string to convert to a C-style string.</param>
        /// <returns>A C-style string for which the caller is responsible.</returns>
        public static byte* ToCString(String str)
        {
            // TODO: actually implement proper UTF-16 -> UTF-8 conversion.
            // This naive algorithm only works for some characters.
            // (fortunately, these characters include the ASCII range)

            byte* cStr = (byte*)Marshal.AllocHGlobal(str.Length + 1);
            for (int i = 0; i < str.Length; i++)
            {
                cStr[i] = (byte)str[i];
            }
            cStr[str.Length] = (byte)'\0';
            return cStr;
        }

        private static extern ulong strlen(byte* str);
    }
}

namespace __compiler_rt
{
    /// <summary>
    /// An interface that the compiler can use to manipulate strings.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Creates a string from a character array. Ownership
        /// of the array is explicitly transferred to the string.
        /// </summary>
        /// <param name="array">The string's character array.</param>
        /// <returns>A string.</returns>
        public static System.String StringFromCharArray(char[] array)
        {
            var str = new System.String();
            str.data = array;
            return str;
        }

        /// <summary>
        /// Creates a string from the given null-terminated string
        /// of UTF-8 encoded characters.
        /// </summary>
        /// <param name="ptr">A pointer to the buffer that contains the string.</param>
        /// <returns>A string.</returns>
        public static System.String StringFromCString(byte* ptr)
        {
            return System.String.FromCString(ptr);
        }
    }
}
