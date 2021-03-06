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
        }
    }

    unroll ((NAME) in (
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Char))
    {
        public partial struct NAME
        {
            /// <inheritdoc/>
            public sealed override int GetHashCode()
            {
                return (int)value;
            }
        }
    }

    unroll ((NAME) in (
        Int64,
        UInt64))
    {
        public partial struct NAME
        {
            /// <inheritdoc/>
            public sealed override int GetHashCode()
            {
                return (int)(value >> 32) ^ (int)value;
            }
        }
    }

    public partial struct Char
    {
        /// <summary>
        /// Indicates if the specified character is a high surrogate.
        /// </summary>
        /// <param name="c">The character to examine.</param>
        /// <returns>
        /// <c>true</c> if the numeric value of character ranges from U+D800 through U+DBFF; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsHighSurrogate(char c)
        {
            return c >= (char)0xD800 && c <= (char)0xDBFF;
        }

        /// <summary>
        /// Indicates if the specified character is a low surrogate.
        /// </summary>
        /// <param name="c">The character to examine.</param>
        /// <returns>
        /// <c>true</c> if the numeric value of character ranges from U+DC00 through U+DFFF; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLowSurrogate(char c)
        {
            return c >= (char)0xDC00 && c <= (char)0xDFFF;
        }

        /// <summary>
        /// Indicates if the specified character is a surrogate.
        /// </summary>
        /// <param name="c">The character to examine.</param>
        /// <returns>
        /// <c>true</c> if the numeric value of character ranges from U+D800 through U+DFFF; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSurrogate(char c)
        {
            return c >= (char)0xD800 && c <= (char)0xDFFF;
        }
    }

    public partial struct SByte
    {
        public const sbyte MaxValue = 0x7F;
        public const sbyte MinValue = (sbyte)0x80;
    }

    public partial struct Byte
    {
        public const byte MaxValue = 0xFF;
        public const byte MinValue = 0x00;
    }

    public partial struct Int16
    {
        public const short MaxValue = 0x7FFF;
        public const short MinValue = (short)0x8000;
    }

    public partial struct UInt16
    {
        public const ushort MaxValue = 0xFFFF;
        public const ushort MinValue = 0x0000;
    }

    public partial struct Int32
    {
        public const int MaxValue = 0x7FFFFFFF;
        public const int MinValue = (int)0x80000000;
    }

    public partial struct UInt32
    {
        public const uint MaxValue = 0xFFFFFFFF;
        public const uint MinValue = 0x00000000;
    }

    public partial struct Int64
    {
        public const long MaxValue = 0x7FFFFFFFFFFFFFFF;
        public const long MinValue = (long)0x8000000000000000;
    }

    public partial struct UInt64
    {
        public const ulong MaxValue = 0xFFFFFFFFFFFFFFFF;
        public const ulong MinValue = 0x0000000000000000;
    }
}