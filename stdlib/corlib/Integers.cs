// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

namespace System
{
    unroll ((TYPE, NAME, DESCRIPTION) in (
        (sbyte, SByte, "a signed 8-bit integer"),
        (byte, Byte, "an unsigned 8-bit integer"),
        (short, Int16, "a signed 16-bit integer"),
        (ushort, UInt16, "an unsigned 16-bit integer"),
        (int, Int32, "a signed 32-bit integer"),
        (uint, UInt32, "an unsigned 32-bit integer"),
        (long, Int64, "a signed 64-bit integer"),
        (ulong, UInt64, "an unsigned 64-bit integer"),
        (char, Char, "a UTF-16 character")))
    {
        [#trivia_doc_comment("<summary>\nRepresents " + DESCRIPTION + ".\n</summary>")]
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