namespace System.Text
{
    public class UnicodeEncoding : Encoding
    {
        public UnicodeEncoding()
            : this(false, false)
        { }

        public UnicodeEncoding(bool isBigEndian, bool includeBOM)
        {
            this.isBigEndian = isBigEndian;
            this.includeBOM = includeBOM;
        }

        private bool isBigEndian;
        private bool includeBOM;

        private bool MustSwitchEndianness => BitConverter.IsLittleEndian == isBigEndian;

        /// <inheritdoc/>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return GetMaxByteCount(count);
        }

        /// <inheritdoc/>
        public override unsafe int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex)
        {
            if (MustSwitchEndianness)
            {
                for (int i = 0; i < charCount; i++)
                {
                    char c = chars[i + charIndex];
                    byte low = (byte)c;
                    byte high = (byte)((int)c >> 8);
                    if (isBigEndian)
                    {
                        bytes[byteIndex + i * 2] = high;
                        bytes[byteIndex + i * 2 + 1] = low;
                    }
                    else
                    {
                        bytes[byteIndex + i * 2] = low;
                        bytes[byteIndex + i * 2 + 1] = high;
                    }
                }
            }
            else
            {
                Buffer.MemoryCopy(
                    &chars[charIndex],
                    &bytes[byteIndex],
                    bytes.Length,
                    charCount * 2);
            }
        }

        /// <inheritdoc/>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetMaxCharCount(count);
        }

        /// <inheritdoc/>
        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            if (MustSwitchEndianness)
            {
                for (int i = 0; i < byteCount / 2; i++)
                {
                    chars[i] = (char)bytes[i * 2 + 1];
                    chars[i] |= (char)((int)bytes[i * 2] << 8);
                }
            }
            else
            {
                Buffer.MemoryCopy(
                    &bytes[byteIndex],
                    &chars[charIndex],
                    chars.Length * 2,
                    byteCount);
            }
        }

        /// <inheritdoc/>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 2;
        }

        /// <inheritdoc/>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount / 2;
        }
    }
}