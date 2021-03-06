using System.Primitives.IO;
using System.Runtime.InteropServices;

namespace System.IO
{
    /// <summary>
    /// A stream that accesses a file.
    /// </summary>
    public unsafe class FileStream : Stream
    {
        /// <summary>
        /// Creates a file stream from the given handle and access.
        /// </summary>
        /// <param name="fileHandle">A file handle.</param>
        /// <param name="access">A file access specifier</param>
        public FileStream(IntPtr fileHandle, FileAccess access)
        {
            if (fileHandle == IntPtr.Zero)
            {
                throw new ArgumentNullException("fileHandle");
            }

            this.Name = null;
            this.access = access;
            this.fileHandle = fileHandle.ToPointer();
        }

        public FileStream(string name, FileMode mode)
            : this(
                name,
                mode,
                mode == FileMode.Append
                    ? FileAccess.Write
                    : FileAccess.ReadWrite)
        { }

        public FileStream(string name, FileMode mode, FileAccess access)
        {
            this.Name = Name;
            this.access = access;
            this.fileHandle = OpenFile(name, mode, access);
            if (fileHandle == null)
            {
                throw new IOException("Cannot open file at '" + name + "'.");
            }
        }

        private FileAccess access;
        private void* fileHandle;

        /// <summary>
        /// Gets the name of the file, as passed to the constructor.
        /// </summary>
        /// <returns>The file name.</returns>
        public string Name { get; private set; }

        private bool IsOpen => fileHandle != null;

        /// <inheritdoc/>
        public override bool CanRead => IsOpen && (access & FileAccess.Read) == FileAccess.Read;

        /// <inheritdoc/>
        public override bool CanWrite => IsOpen && (access & FileAccess.Write) == FileAccess.Write;

        private static void* OpenFile(string name, FileMode mode, FileAccess access)
        {
            // TODO: the special cases are not thread-safe and certainly
            // not atomic. How can we handle this?

            // Handle special cases first. These special cases call
            // `OpenFile` recursively, but the `FileMode` they pass is
            // never a special case---so we won't have infinite recusion
            // here.
            if (mode == FileMode.Truncate)
            {
                if (!FileExists(name))
                {
                    throw new IOException(
                        "Cannot truncate file at '" + name +
                        "' because it does not exist.");
                }
                return OpenFile(name, FileMode.Open, access);
            }
            else if (mode == FileMode.CreateNew)
            {
                if (FileExists(name))
                {
                    throw new IOException(
                        "Cannot create a new file at '" + name +
                        "' as one already exists.");
                }
                return OpenFile(name, FileMode.Create, access);
            }
            else if (mode == FileMode.OpenOrCreate)
            {
                return OpenFile(name, FileExists(name) ? FileMode.Open : FileMode.Create, access);
            }

            bool canRead = (access & FileAccess.Read) == FileAccess.Read;
            bool canWrite = (access & FileAccess.Write) == FileAccess.Write;

            // Handle basic open/create/append operations here.
            switch (mode)
            {
                case FileMode.Open:
                    return OpenFile(name, canWrite ? "r+b" : "rb");

                case FileMode.Create:
                    return OpenFile(name, canRead ? "w+b" : "wb");

                case FileMode.Append:
                    return OpenFile(name, canRead ? "a+b" : "ab");

                default:
                    throw new NotImplementedException();
            }
        }

        private static void* OpenFile(string name, string mode)
        {
            IntPtr nameCStr = Marshal.StringToHGlobalAnsi(name);
            IntPtr modeCStr = Marshal.StringToHGlobalAnsi(mode);

            void* handle = IOPrimitives.OpenFile(
                (byte*)nameCStr.ToPointer(),
                (byte*)modeCStr.ToPointer());

            Marshal.FreeHGlobal(modeCStr);
            Marshal.FreeHGlobal(nameCStr);

            return handle;
        }

        /// <summary>
        /// Checks if the file with the given name exists.
        /// </summary>
        /// <param name="name">The name of the file to check.</param>
        /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
        private static bool FileExists(string name)
        {
            void* handle = OpenFile(name, "r");
            if (handle == null)
            {
                // TODO: maybe look at errno?
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                // Check if seeking to the current position works.
                return IsOpen && IOPrimitives.FileSeek(fileHandle, 0, (int)SeekOrigin.Current) == 0;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                // TODO: error checking
                var oldPos = Position;
                var size = Seek(0, SeekOrigin.End);
                Seek(oldPos, SeekOrigin.Begin);
                return size;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                return IOPrimitives.GetFilePosition(fileHandle);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
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
            EnsureOpen();
            IOPrimitives.FlushFile(fileHandle);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureOpen();
            var error = IOPrimitives.FileSeek(fileHandle, offset, (int)origin);
            if (error != 0)
            {
                throw new IOException(
                    "Cannot seek to position at offset '" + offset +
                    "' from the " + SeekOriginToString(origin) +
                    ". Error code: '" + error + "'.");
            }
            return Position;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckReadArgs(buffer, offset, count);
            EnsureReadable();
            return (int)IOPrimitives.ReadFromFile(&buffer[offset], (ulong)count, fileHandle);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckReadArgs(buffer, offset, count);
            EnsureWriteable();
            ulong bytesWritten = IOPrimitives.WriteToFile(&buffer[offset], (ulong)count, fileHandle);
            if (bytesWritten < (ulong)count)
            {
                throw new IOException("Wrote only " + bytesWritten + " of " + count + " bytes.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsOpen)
            {
                IOPrimitives.CloseFile(fileHandle);
                fileHandle = null;
            }
        }

        private void EnsureOpen()
        {
            if (!IsOpen)
                throw new ObjectDisposedException("Stream has already been closed.");
        }
    }
}