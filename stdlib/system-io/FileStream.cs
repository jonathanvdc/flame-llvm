namespace System.IO
{
    /// <summary>
    /// A stream that accesses a file.
    /// </summary>
    public class FileStream : Stream
    {
        public FileStream(string name, FileMode mode, FileAccess access)
        {
            this.Name = Name;
            this.access = access;
            // TODO: actually open the stream
        }

        private FileAccess access;

        /// <summary>
        /// Gets the name of the file, as passed to the constructor.
        /// </summary>
        /// <returns>The file name.</returns>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override bool CanRead => (access & FileAccess.Read) == FileAccess.Read;

        /// <inheritdoc/>
        public override bool CanWrite => (access & FileAccess.Write) == FileAccess.Write;

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}