namespace System.Primitives.Text
{
    /// <summary>
    /// Contains functionality that helps parse UTF-8--encoded text.
    /// </summary>
    public static unsafe class UnicodeConverter
    {
        // The UTF-8 decoding logic here is based on the Yuu source
        // code by @proxypoke (https://github.com/proxypoke), licensed
        // under the [WTFPLv2](http://sam.zoy.org/wtfpl/COPYING).
        // Repo URL: https://github.com/proxypoke/yuu
        //
        // The UTF-8 encoding and UTF-16 decoding/encoding logic here is
        // based on Pietro Gagliardi's utf library (https://github.com/andlabs),
        // licensed under the MIT license
        // (https://github.com/andlabs/utf/blob/master/LICENSE).
        // Repo URL: https://github.com/andlabs/utf

        private const int BadCodePoint = 0xFFFD;

        private const byte ASCII = 128;
        private const byte CONT = 192;
        private const byte LEAD2 = 224;
        private const byte LEAD3 = 240;
        private const byte LEAD4 = 245;

        private static int bytetype(byte c)
        {
            if (c < ASCII)
            {
                return ASCII;
            }
            else if (c < CONT)
            {
                return CONT;
            }
            else if (c == 192 || c == 193)
            {
                /* 192 and 193 are invalid 2 byte sequences */
                return -1;
            }
            else if (c < LEAD2)
            {
                return LEAD2;
            }
            else if (c < LEAD3)
            {
                return LEAD3;
            }
            else if (c < LEAD4)
            {
                return LEAD4;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Reads a UTF-8--encoded code point from the given buffer.
        /// </summary>
        /// <param name="data">The buffer to advance and read from.</param>
        /// <param name="end">The tail of the buffer to read from.</param>
        /// <param name="eof">
        /// Tells if the end of the buffer was reached before a code point could be decoded.
        /// </param>
        /// <returns>A code point.</returns>
        public static uint ReadUtf8CodePoint(ref byte* data, byte* end, out bool eof)
        {
            byte c = *data;
            int count = 0;
            int cont = 0;
            while (data != end)
            {
                c = *data;
                if (cont == 0)
                {
                    switch (bytetype(c))
                    {
                        case ASCII:
                            count = c;
                            break;
                        case LEAD2:
                            cont = 1;
                            /* Doing a bitwise AND with the complement of a LEAD byte
                            * will cause the bits belonging to the LEAD byte to be
                            * eliminated, leaving the bits we care about.
                            */
                            count = c & (~LEAD2);
                            break;
                        case LEAD3:
                            cont = 2;
                            count = c & (~LEAD3);
                            break;
                        case LEAD4:
                            cont = 3;
                            count = c & (~LEAD4);
                            break;
                        default:
                            /* either an unexpected CONT byte showed up
                            * or an invalid byte.
                            */
                            count = BadCodePoint;
                            break;
                    }
                }
                else
                {
                    switch (bytetype(c))
                    {
                        case CONT:
                            count <<= 6; /* room for 6 more bits */
                            count += c & (~CONT);
                            --cont;
                            break;
                        default:
                            /* There was something other than a CONT byte, either
                            * an invalid byte or an unexpected LEAD or ASCII byte
                            */
                            count = BadCodePoint;
                            break;
                    }
                }

                data++;
                if (cont == 0)
                {
                    eof = false;
                    return (uint)count;
                }
            }
            eof = true;
            return BadCodePoint;
        }

        /// <summary>
        /// Reads a UTF-8--encoded code point from the given buffer.
        /// </summary>
        /// <param name="data">The buffer to advance and read from.</param>
        /// <param name="end">The tail of the buffer to read from.</param>
        /// <returns>A code point.</returns>
        public static uint ReadUtf8CodePoint(ref byte* data, byte* end)
        {
            bool eof;
            return ReadUtf8CodePoint(ref data, end, out eof);
        }

        /// <summary>
        /// Writes a code point to a UTF-8--encoded buffer.
        /// </summary>
        /// <param name="codePoint">The code point to write.</param>
        /// <param name="buffer">
        /// The buffer to write the code point to.
        /// If this buffer is <c>null</c>, then no bytes are written.</param>
        /// <returns>The number of bytes that are used to encode the code point in UTF-8.</returns>
        public static int WriteUtf8CodePoint(uint codePoint, byte* buffer)
        {
            byte b = 0, c = 0, d = 0, e = 0;
            int n = 0;
            do
            {
                // not in the valid range for Unicode
                if (codePoint > 0x10FFFF)
                {
                    codePoint = BadCodePoint;
                }

                // surrogate code points cannot be encoded
                if (codePoint >= 0xD800 && codePoint < 0xE000)
                {
                    codePoint = BadCodePoint;
                }

                if (codePoint < 0x80)
                {
                    // ASCII bytes represent themselves
                    b = (byte)(codePoint & 0xFF);
                    n = 1;
                    break;
                }

                if (codePoint < 0x800)
                {
                    // two-byte encoding
                    c = (byte)(codePoint & 0x3F);
                    c |= 0x80;
                    codePoint >>= 6;
                    b = (byte)(codePoint & 0x1F);
                    b |= 0xC0;
                    n = 2;
                    break;
                }

                if (codePoint < 0x10000)
                {
                    // three-byte encoding
                    d = (byte)(codePoint & 0x3F);
                    d |= 0x80;
                    codePoint >>= 6;
                    c = (byte)(codePoint & 0x3F);
                    c |= 0x80;
                    codePoint >>= 6;
                    b = (byte)(codePoint & 0x0F);
                    b |= 0xE0;
                    n = 3;
                    break;
                }

                // otherwise use a four-byte encoding
                e = (byte)(codePoint & 0x3F);
                e |= 0x80;
                codePoint >>= 6;
                d = (byte)(codePoint & 0x3F);
                d |= 0x80;
                codePoint >>= 6;
                c = (byte)(codePoint & 0x3F);
                c |= 0x80;
                codePoint >>= 6;
                b = (byte)(codePoint & 0x07);
                b |= 0xF0;
                n = 4;
            } while (false);

            if (buffer != null)
            {
                buffer[0] = b;
                if (n > 1)
                    buffer[1] = c;
                if (n > 2)
                    buffer[2] = d;
                if (n > 3)
                    buffer[3] = e;
            }

            return n;
        }

        /// <summary>
        /// Reads a code point from the given UTF-16--encoded buffer.
        /// </summary>
        /// <param name="data">The buffer to advance and read from.</param>
        /// <param name="end">The tail of the buffer to read from.</param>
        /// <returns>A code point.</returns>
        public static uint ReadUtf16CodePoint(ref char* data, char* end)
        {
            char high, low;
            uint codePoint;

            if (*data < 0xD800 || *data >= 0xE000)
            {
                // self-representing character
                codePoint = *data;
                data++;
                return codePoint;
            }

            if (*data >= 0xDC00)
            {
                // out-of-order surrogates
                codePoint = BadCodePoint;
                data++;
                return codePoint;
            }

            if (data == end)
            {
                // not enough elements
                codePoint = BadCodePoint;
                data++;
                return codePoint;
            }

            high = *data;
            high &= (char)0x3FF;
            if (data[1] < 0xDC00 || data[1] >= 0xE000)
            {
                // bad surrogate pair
                codePoint = BadCodePoint;
                data++;
                return codePoint;
            }

            data++;
            low = *data;
            data++;
            low &= (char)0x3FF;
            codePoint = high;
            codePoint <<= 10;
            codePoint |= low;
            codePoint += 0x10000;
            return codePoint;
        }

        /// <summary>
        /// Writes a code point to a UTF-16--encoded buffer.
        /// </summary>
        /// <param name="codePoint">The code point to write.</param>
        /// <param name="buffer">The buffer to write the code point to.</param>
        /// <returns>The number of 16-bit integers that were written to the buffer.</returns>
        public static int WriteUtf16CodePoint(uint codePoint, char* buffer)
        {
            // not in the valid range for Unicode
            if (codePoint > 0x10FFFF)
            {
                codePoint = BadCodePoint;
            }

            // surrogate code points cannot be encoded
            if (codePoint >= 0xD800 && codePoint < 0xE000)
            {
                codePoint = BadCodePoint;
            }

            if (codePoint < 0x10000)
            {
                if (buffer != null)
                {
                    buffer[0] = (char)codePoint;
                }
                return 1;
            }

            codePoint -= 0x10000;
            int low = (int)codePoint & 0x3FF;
            codePoint >>= 10;
            int high = (int)codePoint & 0x3FF;
            if (buffer != null)
            {
                buffer[0] = (char)(high | 0xD800);
                buffer[1] = (char)(low | 0xDC00);
            }
            return 2;
        }

        /// <summary>
        /// Gets the length of the UTF-8 buffer required to transcribe a UTF-16
        /// string to a UTF-8 string.
        /// </summary>
        /// <returns>The size of the UTF-8 buffer in bytes.</returns>
        public static int GetUtf16ToUtf8BufferLength(char* begin, char* end)
        {
            int count = 0;
            while (begin != end)
            {
                count += WriteUtf8CodePoint(ReadUtf16CodePoint(ref begin, end), null);
            }
            return count;
        }
    }
}