// Code based on https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/IO/Stream.cs,
// licensed from the .NET foundation under the MIT license.

namespace System.IO
{
    public abstract class Stream : IDisposable
    {
        public abstract bool CanRead { get; }

        // If CanSeek is false, Position, Seek, Length, and SetLength should throw.
        public abstract bool CanSeek { get; }

        public abstract bool CanWrite { get; }

        public abstract long Length { get; }

        public abstract long Position { get; set; }

        private const string timeoutsNotSupportedMessage = "Timeouts are not supported on this stream.";

        public virtual bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public virtual int ReadTimeout
        {
            get
            {
                throw new InvalidOperationException(timeoutsNotSupportedMessage);
            }
            set
            {
                throw new InvalidOperationException(timeoutsNotSupportedMessage);
            }
        }

        public virtual int WriteTimeout
        {
            get
            {
                throw new InvalidOperationException(timeoutsNotSupportedMessage);
            }
            set
            {
                throw new InvalidOperationException(timeoutsNotSupportedMessage);
            }
        }

        // Reads the bytes from the current stream and writes the bytes to
        // the destination stream until all bytes are read, starting at
        // the current position.
        public void CopyTo(Stream destination)
        {
            int bufferSize = GetCopyBufferSize();

            CopyTo(destination, bufferSize);
        }

        private const string streamClosedMessage = "Cannot access a closed Stream.";

        public virtual void CopyTo(Stream destination, int bufferSize)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "Positive number required.");
            }

            bool sourceCanRead = this.CanRead;
            if (!sourceCanRead && !this.CanWrite)
            {
                throw new ObjectDisposedException(null, streamClosedMessage);
            }

            bool destinationCanWrite = destination.CanWrite;
            if (!destinationCanWrite && !destination.CanRead)
            {
                throw new ObjectDisposedException(nameof(destination), streamClosedMessage);
            }

            if (!sourceCanRead)
            {
                throw new NotSupportedException("Copying data from an unreadable stream is not supported.");
            }

            if (!destinationCanWrite)
            {
                throw new NotSupportedException("Copying data to an unwritable stream is not supported.");
            }

            byte[] buffer = new byte[bufferSize];
            int highwaterMark = 0;
            int read;
            while ((read = Read(buffer, 0, buffer.Length)) != 0)
            {
                if (read > highwaterMark) highwaterMark = read;
                destination.Write(buffer, 0, read);
            }
        }

        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int defaultCopyBufferSize = 81920;

        private int GetCopyBufferSize()
        {
            int bufferSize = defaultCopyBufferSize;

            if (CanSeek)
            {
                long length = Length;
                long position = Position;
                if (length <= position) // Handles negative overflows
                {
                    // There are no bytes left in the stream to copy.
                    // However, because CopyTo{Async} is virtual, we need to
                    // ensure that any override is still invoked to provide its
                    // own validation, so we use the smallest legal buffer size here.
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0)
                    {
                        // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                    }
                }
            }

            return bufferSize;
        }

        // Stream used to require that all cleanup logic went into Close(),
        // which was thought up before we invented IDisposable.  However, we
        // need to follow the IDisposable pattern so that users can write 
        // sensible subclasses without needing to inspect all their base 
        // classes, and without worrying about version brittleness, from a
        // base class switching to the Dispose pattern.  We're moving
        // Stream to the Dispose(bool) pattern - that's where all subclasses 
        // should put their cleanup starting in V2.
        public virtual void Close()
        {
            // Ideally we would assert CanRead == CanWrite == CanSeek = false, 
            // but we'd have to fix PipeStream & NetworkStream very carefully.

            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            // Ideally we would assert CanRead == CanWrite == CanSeek = false, 
            // but we'd have to fix PipeStream & NetworkStream very carefully.

            Close();
        }


        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Synchronizes the stream with the underlying storage.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Seeks to an offset from an origin in a file.
        /// </summary>
        /// <param name="offset">An offset from an origin.</param>
        /// <param name="origin">An origin to seek from.</param>
        /// <returns>The new position in the file.</returns>
        public abstract long Seek(long offset, SeekOrigin origin);

        public abstract void SetLength(long value);

        public abstract int Read(byte[] buffer, int offset, int count);

        // Reads one byte from the stream by calling Read(byte[], int, int). 
        // Will return an unsigned byte cast to an int or -1 on end of stream.
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any 
        // subclass that maintains an internal buffer. Then, it can help perf
        // significantly for people who are reading one byte at a time.
        public virtual int ReadByte()
        {
            byte[] oneByteArray = new byte[1];
            int r = Read(oneByteArray, 0, 1);
            if (r == 0)
                return -1;
            return oneByteArray[0];
        }

        public abstract void Write(byte[] buffer, int offset, int count);

        // Writes one byte from the stream by calling Write(byte[], int, int).
        // This implementation does not perform well because it allocates a new
        // byte[] each time you call it, and should be overridden by any 
        // subclass that maintains an internal buffer. Then, it can help perf
        // significantly for people who are writing one byte at a time.
        public virtual void WriteByte(byte value)
        {
            byte[] oneByteArray = new byte[1];
            oneByteArray[0] = value;
            Write(oneByteArray, 0, 1);
        }

        /// <summary>
        /// Checks the arguments to a read call.
        /// </summary>
        internal static void CheckReadArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "buffer cannot be null.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "offset cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "count cannot be negative.");
            if (buffer.Length - offset < count)
                throw new ArgumentException("A non-existent range of the buffer was specified.");
        }

        internal void EnsureWriteable()
        {
            if (!CanWrite)
                throw new NotSupportedException("Writing is not enabled for this stream.");
        }

        internal void EnsureReadable()
        {
            if (!CanRead)
                throw new NotSupportedException("Reading is not enabled for this stream.");
        }

        internal static string SeekOriginToString(SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return "start of the file";
                case SeekOrigin.Current:
                    return "current position";
                case SeekOrigin.End:
                    return "end of the file";
                default:
                    return "unknown origin (value: '" + origin + "')";
            }
        }
    }
}