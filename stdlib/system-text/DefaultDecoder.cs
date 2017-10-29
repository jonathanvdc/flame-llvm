namespace System.Text
{
    /// <summary>
    /// A default Dencoder implementation that wraps an Encoding.
    /// </summary>
    internal sealed class DefaultDecoder : Decoder
    {
        public DefaultDecoder(Encoding encoding)
        {
            this.encoding = encoding;
        }

        private Encoding encoding;

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return encoding.GetCharCount(bytes, index, count);
        }

        public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            return encoding.GetCharCount(bytes, index, count);
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            return encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, bool flush)
        {
            return encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }
    }
}