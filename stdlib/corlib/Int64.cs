namespace System
{
    /// <summary>
    /// Represents a 64-bit integer.
    /// </summary>
    public struct Int64 : Object, IEquatable<Int64>
    {
        // Note: integers are equivalent to instances of this data structure because
        // flame-llvm stores the contents of single-field structs as a value of their
        // field, rather than as an LLVM struct. So a 64-bit integer becomes an i64 and
        // so does a `System.Int64`. So don't add, remove, or edit the fields in this
        // struct.
        private long value;

        /// <summary>
        /// Converts this integer to a string representation.
        /// </summary>
        /// <returns>The string representation for the integer.</returns>
        public sealed override string ToString()
        {
            return Convert.ToString(value);
        }

        /// <inheritdoc/>
        public bool Equals(Int64 other)
        {
            return value == other.value;
        }

        /// <inheritdoc/>
        public sealed override bool Equals(Object other)
        {
            return other is Int64 && Equals((Int64)other);
        }

        /// <inheritdoc/>
        public sealed override int GetHashCode()
        {
            return (int)value;
        }
    }
}