namespace System.Text
{
    /// <summary>
    /// A base class for character codecs.
    /// </summary>
    public abstract class Encoding
    {
        /// <summary>
        /// Returns the number of bytes required to encode a range of characters in
        /// a character array.
        /// </summary>
        /// <param name="chars">The characters to encode.</param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>The number of bytes required to encode the selected characters.</returns>
        public abstract int GetByteCount(char[] chars, int index, int count);

        public abstract int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex);

        public abstract int GetCharCount(byte[] bytes, int index, int count);

        public abstract int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex);

        /// <summary>
        /// Gets the maximum number of bytes required for decoding a given number
        /// of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to decode.</param>
        /// <returns>The maximum number of bytes required for decoding <c>charCount</c> bytes.</returns>
        public abstract int GetMaxByteCount(int charCount);

        /// <summary>
        /// Gets the maximum number of characters produced by decoding a given number
        /// of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>The maximum number of characters produced by decoding <c>byteCount</c> bytes.</returns>
        public abstract int GetMaxCharCount(int byteCount);
    }
}