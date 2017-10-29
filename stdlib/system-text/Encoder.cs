namespace System.Text
{
    /// <summary>
    /// A base class for objects that convert arrays of characters to bytes.
    /// </summary>
    public abstract class Encoder
    {
        public abstract int GetBytes(
            char[] chars, int charIndex, int charCount,
            byte[] bytes, int byteIndex, bool flush);

        public abstract int GetByteCount(
            char[] chars, int index, int count, bool flush);

        public virtual void Reset()
        {

        }
    }
}