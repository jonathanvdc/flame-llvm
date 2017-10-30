namespace System.Text
{
    /// <summary>
    /// A base class for objects that convert arrays of bytes to characters.
    /// </summary>
    public abstract class Decoder
    {
        public abstract int GetCharCount(byte[] bytes, int index, int count);

        public virtual int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            return GetCharCount(bytes, index, count);
        }

        public abstract int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex);

        public virtual int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, bool flush)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public virtual void Reset()
        {

        }
    }
}