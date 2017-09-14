// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

namespace System
{
    unroll ((TYPE, NAME, DESCRIPTION) in (
        (float, Single, "a 32-bit floating-point number"),
        (double, Double, "a 64-bit floating-point number")))
    {
        [#trivia_doc_comment("<summary>\nRepresents " + DESCRIPTION + ".\n</summary>")]
        public partial struct NAME : Object, IEquatable<NAME>
        {
            // Note: integers are equivalent to instances of this data structure because
            // flame-llvm stores the contents of single-field structs as a value of their
            // field, rather than as an LLVM struct. So a 64-bit integer becomes an f64 and
            // so does a `System.Double`. So don't add, remove, or edit the fields in this
            // struct.
            private TYPE value;

            /// <summary>
            /// Converts this integer to a string representation.
            /// </summary>
            /// <returns>The string representation for the integer.</returns>
            public sealed override string ToString()
            {
                return Convert.ToString(value);
            }

            /// <inheritdoc/>
            public bool Equals(NAME other)
            {
                return value == other.value;
            }

            /// <inheritdoc/>
            public sealed override bool Equals(Object other)
            {
                return other is NAME && Equals((NAME)other);
            }
        }
    }

    public partial struct Single
    {
        /// <inheritdoc/>
        public sealed override unsafe int GetHashCode()
        {
            return BitConverter.SingleToInt32Bits(value).GetHashCode();
        }
    }

    public partial struct Double
    {
        /// <inheritdoc/>
        public sealed override unsafe int GetHashCode()
        {
            return BitConverter.DoubleToInt64Bits(value).GetHashCode();
        }
    }
}