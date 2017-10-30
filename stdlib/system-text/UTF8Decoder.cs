using System.Primitives.Text;

namespace System.Text
{
    public sealed class UTF8Decoder : Decoder
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
            // Cache the state of the undecoded byte buffer.
            byte b0 = undecodedBytes[0];
            byte b1 = undecodedBytes[1];
            byte b2 = undecodedBytes[2];
            byte b3 = undecodedBytes[3];
            int oldUndecodedByteCount = undecodedByteCount;

            if (flush)
            {
                FlushCache();
            }

            // Decode the bytes.
            int result = GetCharsImpl(bytes, index, count, null, 0);

            // Restore the state of the undecoded byte buffer.
            undecodedBytes[0] = b0;
            undecodedBytes[1] = b1;
            undecodedBytes[2] = b2;
            undecodedBytes[3] = b3;
            undecodedByteCount = oldUndecodedByteCount;

            return result;
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, true);
        }

        public override int GetChars(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, bool flush)
        {
            if (flush)
            {
                FlushCache();
            }

            return GetCharsImpl(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override void Reset()
        {
            FlushCache();
        }

        private int GetCharsImpl(
            byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex)
        {
            // TODO: check that we're not overflowing any of these buffers here.

            int numCharsParsed = 0;
            byte* curPtr = &bytes[byteIndex];
            byte* endPtr = &bytes[byteIndex + byteCount];
            byte* undecodedByteStartPtr = &undecodedBytes[0];
            byte* undecodedBytePtr;
            bool eofReached;

            if (HasUndecodedBytes)
            {
                // Try to parse undecoded bytes by appending decoded bytes.

                int newUndecodedByteCount = Math.Min(undecodedByteCount + byteCount, undecodedBytes.Length);
                for (int i = undecodedByteCount; i < newUndecodedByteCount; i++)
                {
                    undecodedBytes[i] = curPtr[i - undecodedByteCount];
                }

                undecodedBytePtr = undecodedByteStartPtr;
                byte* undecodedByteEndPtr = undecodedBytePtr + newUndecodedByteCount;
                uint codePoint = UnicodeConverter.ReadUtf8CodePoint(
                    ref undecodedBytePtr, undecodedByteEndPtr, out eofReached);

                if (eofReached)
                {
                    // We don't have enough new bytes to decode a code point.
                    undecodedByteCount = newUndecodedByteCount;
                    return 0;
                }

                // We can compute the total number of *new* bytes we've decoded by first
                // computing the total number of bytes decoded `undecodedByteEndPtr - undecodedBytePtr`
                // and the subtracting the number of undecoded bytes.
                long numNewBytesDecoded = (long)undecodedByteEndPtr - (long)undecodedBytePtr - undecodedByteCount;
                curPtr += numNewBytesDecoded;
                numCharsParsed += WriteToCharBuffer(codePoint, chars, charIndex + numCharsParsed);
                undecodedByteCount = 0;
            }

            while (curPtr != endPtr)
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

                numCharsParsed += WriteToCharBuffer(codePoint, chars, charIndex + numCharsParsed);
            }

            // Copy undecoded bytes to the undecoded byte buffer.
            undecodedBytePtr = undecodedByteStartPtr;
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

        private void FlushCache()
        {
            this.undecodedByteCount = 0;
        }

        private static int WriteToCharBuffer(uint codePoint, char[] buffer, int offset)
        {
            return UnicodeConverter.WriteUtf16CodePoint(codePoint, buffer == null ? (char*)null : &buffer[offset]);
        }
    }
}