namespace System
{
    /// <summary>
    /// Represents a 32-bit integer.
    /// </summary>
    public struct Int32
    {
        // Note: integers are equivalent to instances of this data structure because
        // flame-llvm the contents of single-field structs as a value of their single
        // field, rather than as a LLVM struct. So a 32-bit integer becomes an i32 and
        // so does a `System.Int32`. So don't add, remove, or edit the fields in this
        // struct.
        private int value;

        /// <summary>
        /// Converts this integer to a string representation.
        /// </summary>
        /// <returns>The string representation for the integer.</returns>
        public string ToString()
        {
            return Convert.ToString(value);
        }
    }
}