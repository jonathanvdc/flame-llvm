namespace System
{
    /// <summary>
    /// Contains functionality that helps parse UTF-8--encoded text.
    /// </summary>
    internal static class UnicodeConverter
    {
        // The UTF-8 decoding logic here is based on the Yuu source
        // code by @proxypoke (https://github.com/proxypoke), licensed
        // under the [WTFPLv2](http://sam.zoy.org/wtfpl/COPYING).
        // Repo URL: https://github.com/proxypoke/yuu
        //
        // The UTF-16 encoding logic here is based on Pietro Gagliardi's
        // utf library (https://github.com/andlabs), licensed under
        // the MIT license (https://github.com/andlabs/utf/blob/master/LICENSE).
        // Repo URL: https://github.com/andlabs/utf

        private const byte ASCII = 128;
        private const byte CONT = 192;
        private const byte LEAD2 = 224;
        private const byte LEAD3 = 240;
        private const byte LEAD4 = 245;

        private static int bytetype(byte c)
        {
            if (c < ASCII)
            {
                // printf("%d - ASCII\n", c);
                return ASCII;
            }
            else if (c < CONT)
            {
                // printf("%d - CONT\n", c);
                return CONT;
            }
            else if (c == 192 || c == 193)
            {
                /* 192 and 193 are invalid 2 byte sequences */
                // printf("%d - INVALID\n", c);
                throw new Object();
            }
            else if (c < LEAD2)
            {
                // printf("%d - LEAD2\n", c);
                return LEAD2;
            }
            else if (c < LEAD3)
            {
                // printf("%d - LEAD3\n", c);
                return LEAD3;
            }
            else if (c < LEAD4)
            {
                // printf("%d - LEAD4\n", c);
                return LEAD4;
            }
            else
            {
                // printf("%d - INVALID\n", c);
                throw new Object();
            }
        }

        /// <summary>
        /// Reads a UTF-8--encoded code point from the given buffer.
        /// </summary>
        /// <param name="data">The buffer to advance and read from.</param>
        /// <param name="end">The tail of the buffer to read from.</param>
        /// <returns>A code point.</returns>
        public static unsafe uint ReadUtf8CodePoint(ref byte* data, byte* end)
        {
            byte c = *data;
            int result = 0;
            int cont = 0;
            while (data != end)
            {
                c = *data;
                if (cont == 0)
                {
                    switch (bytetype(c))
                    {
                        case ASCII:
                            result = c;
                            break;
                        case LEAD2:
                            cont = 1;
                            /* Doing a bitwise AND with the complement of a LEAD byte
                            * will cause the bits belonging to the LEAD byte to be
                            * eliminated, leaving the bits we care about.
                            */
                            result = c & (~LEAD2);
                            break;
                        case LEAD3:
                            cont = 2;
                            result = c & (~LEAD3);
                            break;
                        case LEAD4:
                            cont = 3;
                            result = c & (~LEAD4);
                            break;
                        default:
                            /* either an unexpected CONT byte showed up
                            * or an invalid byte.
                            * TODO: Error handling
                            */
                            // printf("\nInvalid byte or unexpected CONT byte.\n");
                            throw new Object();
                    }
                }
                else
                {
                    switch (bytetype(c))
                    {
                        case CONT:
                            result <<= 6; /* room for 6 more bits */
                            result += c & (~CONT);
                            --cont;
                            break;
                        default:
                            /* There was something other than a CONT byte, either
                            * an invalid byte or an unexpected LEAD or ASCII byte
                            * TODO: Error handling
                            */
                            // printf("Invalid byte or unexpected non-CONT byte.\n");
                            throw new Object();
                    }
                }

                data++;
                if (cont == 0)
                {
                    return (uint)result;
                }
            }
        }

        /// <summary>
        /// Writes a code point to a UTF-16--encoded buffer.
        /// </summary>
        /// <param name="codePoint">The code point to write.</param>
        /// <param name="buffer">The buffer to write the code point to.</param>
        /// <returns>The number of 16-bit integers that were written to the buffer.</returns>
        public static unsafe int WriteUtf16CodePoint(uint codePoint, char* buffer)
        {
            // not in the valid range for Unicode
            if (codePoint > 0x10FFFF)
            {
                throw new Object();
            }

            // surrogate code points cannot be encoded
            if (codePoint >= 0xD800 && codePoint < 0xE000)
            {
                throw new Object();
            }

            if (codePoint < 0x10000)
            {
                buffer[0] = (char)codePoint;
                return 1;
            }

            codePoint -= 0x10000;
            int low = (int)codePoint & 0x3FF;
            codePoint >>= 10;
            int high = (int)codePoint & 0x3FF;
            buffer[0] = (char)(high | 0xD800);
            buffer[1] = (char)(low | 0xDC00);
            return 2;
        }
    }
}