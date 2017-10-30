using System.Primitives.Text;

namespace System.Text
{
    public class UTF8Encoding : Encoding
    {
        public UTF8Encoding()
        {
            encoder = new UTF8Encoder();
            decoder = new UTF8Decoder();
        }

        private UTF8Encoder encoder;
        private UTF8Decoder decoder;

        /// <inheritdoc/>
        public override Encoder GetEncoder()
        {
            return new UTF8Encoder();
        }

        /// <inheritdoc/>
        public override Decoder GetDecoder()
        {
            return new UTF8Decoder();
        }

        /// <inheritdoc/>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return encoder.GetByteCount(chars, index, count, true);
        }

        /// <inheritdoc/>
        public override unsafe int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex)
        {
            return encoder.GetBytes(chars, charIndex, charCount, bytes, byteIndex, true);
        }

        /// <inheritdoc/>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return decoder.GetCharCount(bytes, index, count, true);
        }

        /// <inheritdoc/>
        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            return decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex, true);
        }

        /// <inheritdoc/>
        public override int GetMaxByteCount(int charCount)
        {
            return (charCount + 1) * 3;
        }

        /// <inheritdoc/>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount + 1;
        }
    }

    internal sealed class UTF8Encoder : Encoder
    {
        public UTF8Encoder()
        {
            utf16Buffer = new char[2];
            FlushCache();
        }

        private char cachedHighSurrogate;
        private char[] utf16Buffer;

        /// <inheritdoc/>
        public override int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex, bool flush)
        {
            if (flush)
            {
                FlushCache();
            }

            int byteCount = 0;
            for (int i = 0; i < charCount; i++)
            {
                // TODO: check if bytes is big enough to hold the next char
                byteCount += EncodeChar(
                    chars[charIndex + i],
                    &bytes[byteIndex + byteCount]);
            }
            return byteCount;
        }

        /// <inheritdoc/>
        public override int GetByteCount(
            char[] chars, int index, int count, bool flush)
        {
            var oldCache = cachedHighSurrogate;
            if (flush)
            {
                FlushCache();
            }

            int byteCount = 0;
            for (int i = 0; i < count; i++)
            {
                byteCount += EncodeChar(chars[index + i], (byte*)null);
            }

            cachedHighSurrogate = oldCache;
            return byteCount;
        }

        private int EncodeChar(char c, byte* dest)
        {
            char highSurrogate = cachedHighSurrogate;
            if (highSurrogate == '\0')
            {
                if (char.IsHighSurrogate(c))
                {
                    // We'll remember high surrogates instead of printing them
                    // right away. (They need to be matched to a low surrogate,
                    // which is yet to come.)
                    cachedHighSurrogate = c;
                    return 0;
                }
                else
                {
                    // This character represents a single code point---write it
                    // to the output array immediately.
                    char* argBegin = &c;
                    return WriteUtf16CodePoint(argBegin, argBegin + 1, dest);
                }
            }
            else
            {
                cachedHighSurrogate = '\0';
                if (char.IsLowSurrogate(c))
                {
                    // Copy both characters in the surrogate pair into the UTF-16
                    // buffer, transcribe the UTF-16 buffer's contents into UTF-8
                    // and write the result to the output array.
                    utf16Buffer[0] = highSurrogate;
                    utf16Buffer[1] = c;
                    char* argBegin = &utf16Buffer[0];
                    return WriteUtf16CodePoint(argBegin, argBegin + 2, dest);
                }
                else
                {
                    // Bad surrogate pair. Write both characters individually.
                    char* argBegin = &highSurrogate;
                    int bytesWritten = WriteUtf16CodePoint(argBegin, argBegin + 1, dest);
                    argBegin = &c;
                    bytesWritten += WriteUtf16CodePoint(argBegin, argBegin + 1, dest);
                    return bytesWritten;
                }
            }
        }

        public override void Reset()
        {
            FlushCache();
        }

        private void FlushCache()
        {
            cachedHighSurrogate = '\0';
        }

        private static int WriteUtf16CodePoint(
            char* begin, char* end,
            byte* dest)
        {
            return UnicodeConverter.WriteUtf8CodePoint(
                UnicodeConverter.ReadUtf16CodePoint(ref begin, end),
                dest);
        }
    }
}