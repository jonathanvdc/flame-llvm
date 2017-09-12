// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

namespace System
{
    unroll ((TYPE, NAME, DESCRIPTION) in (
        (sbyte, SByte, "a signed 8-bit"),
        (byte, Byte, "an unsigned 8-bit"),
        (short, Int16, "a signed 16-bit"),
        (ushort, UInt16, "an unsigned 16-bit"),
        (int, Int32, "a signed 32-bit"),
        (uint, UInt32, "an unsigned 32-bit"),
        (long, Int64, "a signed 64-bit"),
        (ulong, UInt64, "an unsigned 64-bit")))
    {
        [#trivia_doc_comment("<summary>\nRepresents " + DESCRIPTION + " integer.\n</summary>")]
        public struct NAME : Object, IEquatable<NAME>
        {
            // Note: integers are equivalent to instances of this data structure because
            // flame-llvm stores the contents of single-field structs as a value of their
            // field, rather than as an LLVM struct. So a SIZE-bit integer becomes an iSIZE
            // and so does a `System.NAME`. But don't add, remove, or edit the fields in this
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

            /// <inheritdoc/>
            public sealed override int GetHashCode()
            {
                return (int)value;
            }
        }
    }
}