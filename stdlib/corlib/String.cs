using System.Primitives.InteropServices;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// A character string that uses the UTF-16 encoding.
    /// </summary>
    public sealed class String : Object, IEquatable<String>
    {
        private String()
        { }

        /// <summary>
        /// Creates a string from the given sequence of characters.
        /// </summary>
        /// <param name="characters">The characters the string is composed of.</param>
        public String(char[] characters)
            : this(characters, characters.Length)
        { }

        /// <summary>
        /// Creates a string from the given sequence of characters.
        /// </summary>
        /// <param name="characters">The characters the string is composed of.</param>
        /// <param name="length">The number of characters to copy from the array.</param>
        public String(char[] characters, int length)
        {
            // Copy the array to an array of our own.
            data = new char[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = characters[i];
            }
        }

        private char[] data;

        /// <summary>
        /// Gets this string's length.
        /// </summary>
        public int Length => data.Length;

        /// <summary>
        /// Gets the character at the given position in this string.
        /// </summary>
        public char this[int i] => data[i];

        /// <summary>
        /// Gets a pointer to this string's backing data.
        /// </summary>
        private char* DataPointer => Length == 0 ? null : &data[0];

        /// <summary>
        /// Checks if this string is equal to the given string.
        /// </summary>
        /// <param name="other">The string to compare this string to.</param>
        /// <returns><c>true</c> if this string is equal to the given string; otherwise, <c>false</c>.</returns>
        public bool Equals(String other)
        {
            if (Length != other.Length)
            {
                // Strings of different lengths can't be equal.
                return false;
            }

            if (data == other.data)
            {
                // Strings that use the same underlying buffer are
                // always equal.
                return true;
            }

            for (int i = 0; i < Length; i++)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if this string is equal to the given value.
        /// </summary>
        /// <param name="other">The value to compare this string to.</param>
        /// <returns><c>true</c> if this string is equal to the given value; otherwise, <c>false</c>.</returns>
        public sealed override bool Equals(Object obj)
        {
            return obj is String && Equals((String)obj);
        }

        /// <summary>
        /// Gets a hash code for this string.
        /// </summary>
        /// <returns>A hash code.</returns>
        public sealed override int GetHashCode()
        {
            // This is the `djb2` hash algorithm by Dan Bernstein. It can be
            // found at http://www.cse.yorku.ca/~oz/hash.html

            int hash = 5381;

            for (int i = 0; i < Length; i++)
            {
                hash = ((hash << 5) + hash) + this[i]; /* hash * 33 + c */
            }

            return hash;
        }

        /// <inheritdoc/>
        public sealed override string ToString()
        {
            return this;
        }

        /// <summary>
        /// Concatenates two strings.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <returns>The concatenated string.</returns>
        public static string Concat(string first, string second)
        {
            string result = new String();
            result.data = new char[first.Length + second.Length];
            for (int i = 0; i < first.Length; i++)
            {
                result.data[i] = first.data[i];
            }
            for (int i = 0; i < second.Length; i++)
            {
                result.data[i + first.Length] = second.data[i];
            }
            return result;
        }

        /// <summary>
        /// Creates a string from the given null-terminated string
        /// of UTF-8 encoded characters.
        /// </summary>
        /// <param name="buffer">The buffer that contains the string.</param>
        /// <returns>A string.</returns>
        internal static unsafe string FromCString(byte* buffer)
        {
            int utf8Length = (int)CStringHelpers.StringLength(buffer);
            var utf16Buffer = new char[utf8Length];

            var bufEnd = buffer + utf8Length;
            int utf16Length = 0;
            while (buffer != bufEnd)
            {
                utf16Length += UnicodeConverter.WriteUtf16CodePoint(
                    UnicodeConverter.ReadUtf8CodePoint(ref buffer, bufEnd),
                    &utf16Buffer[utf16Length]);
            }

            return new String(utf16Buffer, utf16Length);
        }

        /// <summary>
        /// Allocates an unmanaged buffer and fills it with this string's contents,
        /// re-encoded as UTF-8. The resulting buffer is terminated by the null
        /// terminator character. The caller is responsible for freeing the buffer
        /// when it's done using it.
        /// </summary>
        /// <param name="str">The string to convert to a C-style string.</param>
        /// <returns>A C-style string for which the caller is responsible.</returns>
        internal static unsafe byte* ToCString(string str)
        {
            char* beginPtr = str.DataPointer;
            char* endPtr = beginPtr + str.Length - 1;
            var utf8Length = UnicodeConverter.GetUtf16ToUtf8BufferLength(beginPtr, endPtr);

            byte* cStr = (byte*)Marshal.AllocHGlobal(utf8Length + 1);
            int offset = 0;
            while (beginPtr != endPtr)
            {
                offset += UnicodeConverter.WriteUtf8CodePoint(
                    UnicodeConverter.ReadUtf16CodePoint(ref beginPtr, endPtr),
                    &cStr[offset]);
            }
            cStr[str.Length] = (byte)'\0';
            return cStr;
        }

        /// <summary>
        /// Creates a string from an (immutable) character array. Ownership
        /// of the array is explicitly transferred to the string.
        /// </summary>
        /// <param name="array">The string's character array.</param>
        /// <returns>A string.</returns>
        /// <remarks>
        /// Only code generated by the compiler should touch this method; it is inaccessible to user code.
        /// </remarks>
        [#builtin_hidden]
        public static string FromConstCharArray(char[] array)
        {
            return new String() { data = array };
        }
    }
}