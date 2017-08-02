namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Defines helper methods to interact with null-terminated strings.
    /// </summary>
    public static unsafe class CStringHelpers
    {
        private static extern ulong strlen(byte* str);

        /// <summary>
        /// Gets the length (in bytes) of the given null-terminated string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The string's length.</returns>
        public static ulong StringLength(byte* str)
        {
            return strlen(str);
        } 
    }
}