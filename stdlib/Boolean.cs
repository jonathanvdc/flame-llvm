namespace System
{
    /// <summary>
    /// Represents a Boolean value.
    /// </summary>
    public struct Boolean : Object, IEquatable<Boolean>
    {
        // Note: Booleans are equivalent to instances of this data structure because
        // flame-llvm stores the contents of single-field structs as a value of their
        // field, rather than as a LLVM struct. So a `bool` and a `System.Boolean`
        // become the same type. But don't add, remove, or edit the fields in this
        // struct!
        private bool value;

        /// <summary>
        /// Converts this Boolean to a string representation.
        /// </summary>
        /// <returns>The string representation for the Boolean.</returns>
        public string ToString()
        {
            return Convert.ToString(value);
        }

        /// <inheritdoc/>
        public bool Equals(Boolean other)
        {
            return value == other.value;
        }
    }
}