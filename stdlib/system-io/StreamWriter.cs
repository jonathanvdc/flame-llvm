using System.Text;

namespace System.IO
{
    public class StreamWriter : TextWriter
    {
        public StreamWriter(Stream stream)
            : this(stream, Encoding.UTF8)
        { }

        public StreamWriter(Stream stream, Encoding encoding)
        {
            this.BaseStream = stream;
            this.codec = encoding;
            this.encoder = encoding.GetEncoder();
        }

        private Encoding codec;
        private Encoder encoder;

        /// <inheritdoc/>
        public override Encoding Encoding => codec;

        /// <summary>
        /// Gets the backing stream for this text writer.
        /// </summary>
        /// <returns>The backing stream.</returns>
        public Stream BaseStream { get; private set; }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            Write(new char[] { value }, 0, 1);
        }

        /// <inheritdoc/>
        public override void Write(char[] buffer, int index, int count)
        {
            int numOfBytes = encoder.GetByteCount(buffer, index, count, false);
            var bytes = new byte[numOfBytes];
            numOfBytes = encoder.GetBytes(buffer, index, count, bytes, 0, false);
            BaseStream.Write(bytes, 0, numOfBytes);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            BaseStream.Dispose();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            BaseStream.Flush();
        }
    }
}