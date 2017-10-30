using System.Primitives.Text;

namespace System.Text
{
    internal sealed class UTF8Decoder : Decoder
    {
        public UTF8Decoder()
        {
            this.undecodedBytes = new byte[4];
            FlushCache();
        }

        private byte[] undecodedBytes;
        private int undecodedByteCount;
        private bool HasUndecodedBytes => undecodedByteCount > 0;

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetCharCount(bytes, index, count, true);
        }

        public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            throw new NotImplementedException();
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            return GetCharCount(bytes, byteIndex, byteCount, chars, charIndex, true);
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, bool flush)
        {
            if (flush)
            {
                FlushCache();
            }

            // TODO: check that we're not overflowing any of these buffers here.

            int numCharsParsed = 0;
            byte* curPtr = &bytes[byteIndex];
            byte* endPtr = &bytes[byteIndex + byteCount];
            byte* undecodedByteStartPtr = &undecodedBytes[0];
            bool eofReached;

            if (HasUndecodedBytes)
            {
                // Try to parse undecoded bytes by appending decoded bytes.

                int newUnencodedByteCount = Math.Min(undecodedBytes.Length, byteCount);
                for (int i = undecodedByteCount; i < newUnencodedByteCount; i++)
                {
                    undecodedBytes[i] = curPtr[i - undecodedByteCount];
                }

                byte* undecodedBytePtr = undecodedByteStartPtr;
                byte* undecodedByteEndPtr = undecodedBytePtr + newUnencodedByteCount;
                uint codePoint = UnicodeConverter.ReadUtf8CodePoint(
                    ref undecodedBytePtr, undecodedByteEndPtr, out eofReached);

                if (!eofReached)
                {
                    // We can compute the total number of *new* bytes we've decoded by first
                    // computing the total number of bytes decoded `undecodedByteEndPtr - undecodedBytePtr`
                    // and the subtracting the number of undecoded bytes.
                    long numNewBytesDecoded = undecodedByteEndPtr - undecodedBytePtr - undecodedByteCount;
                    curPtr += numNewBytesDecoded;
                    numCharsParsed += WriteToCharBuffer(codePoint, charIndex + numCharsParsed);
                }
            }

            while (!eofReached && curPtr != endPtr)
            {
                byte* oldCurPtr = curPtr;
                uint codePoint = UnicodeConverter.ReadUtf8CodePoint(
                    ref curPtr, endPtr, out eofReached);

                if (eofReached)
                {
                    // Stop trying to parse code points; move the rest of the data into
                    // the undecoded bytes buffer.
                    curPtr = oldCurPtr;
                    break;
                }

                numCharsParsed += WriteToCharBuffer(codePoint, charIndex + numCharsParsed);
            }

            undecodedByteCount = 0;
            while (curPtr != endPtr)
            {
                *undecodedBytePtr = *curPtr;
                undecodedBytePtr++;
                curPtr++;
                undecodedByteCount++;
            }

            return numCharsParsed;
        }

        public override void Reset()
        {
            FlushCache();
        }

        private void FlushCache()
        {
            this.undecodedByteCount = 0;
        }
    }
}