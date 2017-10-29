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

        /// <summary>
        /// Encodes a range of characters from a character array as a
        /// range of bytes in a byte array.
        /// </summary>
        /// <param name="chars">The array of characters of which a range is to be encoded.</param>
        /// <param name="charIndex">The index of the first character in the array to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The array of bytes to write encoded data to.</param>
        /// <param name="byteIndex">The index in the byte array at which to start writing.</param>
        /// <returns>The number of bytes that the characters were encoded as.</returns>
        public abstract int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex);

        /// <summary>
        /// Returns the number of characters required to decode a range of bytes in
        /// a byte array.
        /// </summary>
        /// <param name="bytes">The bytes to decode.</param>
        /// <param name="index">The index of the first byte to dencode.</param>
        /// <param name="count">The number of bytes to dencode.</param>
        /// <returns>The number of characters required to dencode the selected bytes.</returns>
        public abstract int GetCharCount(byte[] bytes, int index, int count);

        /// <summary>
        /// Decodes a range of bytes from a byte array as a
        /// range of characters in a characters array.
        /// </summary>
        /// <param name="bytes">The array of bytes of which a range is to be decoded.</param>
        /// <param name="byteIndex">The index of the first byte in the array to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The array of characters to write decoded characters to.</param>
        /// <param name="charIndex">The index in the character array at which to start writing.</param>
        /// <returns>The number of characters that were decoded.</returns>
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

        /// <summary>
        /// Encodes a range of characters from a character array as an array of bytes.
        /// </summary>
        /// <param name="chars">The array of characters of which a range is to be encoded.</param>
        /// <param name="index">The index of the first character in the array to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>An array of bytes.</returns>
        public virtual byte[] GetBytes(char[] chars, int index, int count)
        {
            int byteCount = GetByteCount(chars, index, count);
            var bytes = new byte[byteCount];
            GetBytes(chars, index, count, bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Encodes a character array as an array of bytes.
        /// </summary>
        /// <param name="chars">The array of characters to encode.</param>
        /// <returns>An array of bytes.</returns>
        public virtual byte[] GetBytes(char[] chars)
        {
            return GetBytes(chars, 0, chars.Length);
        }

        /// <summary>
        /// Encodes a character string as an array of bytes.
        /// </summary>
        /// <param name="chars">The character string to encode.</param>
        /// <returns>An array of bytes.</returns>
        public virtual byte[] GetBytes(string chars)
        {
            return GetBytes(chars.ToCharArray());
        }

        /// <summary>
        /// Encodes a range of characters from a character string as an array of bytes.
        /// </summary>
        /// <param name="chars">The string of characters of which a range is to be encoded.</param>
        /// <param name="charIndex">The index of the first character in the array to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The array of bytes to write encoded data to.</param>
        /// <param name="byteIndex">The index in the byte array at which to start writing.</param>
        /// <returns>The number of bytes that the characters were encoded as.</returns>
        public virtual int GetBytes(
            string chars, int index, int count,
            byte[] bytes, int byteIndex)
        {
            return GetBytes(
                chars.ToCharArray(index, count), 0, count,
                bytes, byteIndex);
        }
    }
}