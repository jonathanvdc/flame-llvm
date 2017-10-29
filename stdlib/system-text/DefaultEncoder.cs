namespace System.Text
{
    /// <summary>
    /// A default Encoder implementation that wraps an Encoding.
    /// </summary>
    internal sealed class DefaultEncoder : Encoder
    {
        public DefaultEncoder(Encoding encoding)
        {
            this.encoding = encoding;
        }

        private Encoding encoding;

        /// <inheritdoc/>
        public override int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex, bool flush)
        {
            return encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        /// <inheritdoc/>
        public override int GetByteCount(
            char[] chars, int index, int count, bool flush)
        {
            return encoding.GetByteCount(chars, index, count);
        }
    }
}