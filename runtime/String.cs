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
    }
}
