namespace System
{
    /// <summary>
    /// Represents a 64-bit floating-point number.
    /// </summary>
    public struct Double : Object, IEquatable<Double>
    {
        // Note: integers are equivalent to instances of this data structure because
        // flame-llvm stores the contents of single-field structs as a value of their
        // field, rather than as an LLVM struct. So a 64-bit integer becomes an f64 and
        // so does a `System.Double`. So don't add, remove, or edit the fields in this
        // struct.
        private double value;

        /// <summary>
        /// Converts this integer to a string representation.
        /// </summary>
        /// <returns>The string representation for the integer.</returns>
        public sealed override string ToString()
        {
            return Convert.ToString(value);
        }

        /// <inheritdoc/>
        public bool Equals(Double other)
        {
            return value == other.value;
        }

        /// <inheritdoc/>
        public sealed override bool Equals(Object other)
        {
            return other is Double && Equals((Double)other);
        }

        /// <inheritdoc/>
        public sealed override unsafe int GetHashCode()
        {
            var valPtr = &value;
            var longVal = *((long*)valPtr);
            return longVal.GetHashCode();
        }
    }
}