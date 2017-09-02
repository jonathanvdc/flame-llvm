namespace System
{
    /// <summary>
    /// Represents a 32-bit integer.
    /// </summary>
    public struct Int32 : Object, IEquatable<Int32>
    {
        // Note: integers are equivalent to instances of this data structure because
        // flame-llvm stores the contents of single-field structs as a value of their
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

        /// <inheritdoc/>
        public bool Equals(Int32 other)
        {
            return value == other.value;
        }

        /// <inheritdoc/>
        public override bool Equals(Object other)
        {
            return other is Int32 && Equals((Int32)other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return value;
        }
    }
}