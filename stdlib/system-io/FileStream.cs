using System.Primitives.IO;
using System.Runtime.InteropServices;

namespace System.IO
{
    /// <summary>
    /// A stream that accesses a file.
    /// </summary>
    public unsafe class FileStream : Stream
    {
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
            this.fileHandle = OpenFile(mode);
            if (fileHandle == (void*)null)
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

        /// <inheritdoc/>
        public override bool CanRead => (access & FileAccess.Read) == FileAccess.Read;

        /// <inheritdoc/>
        public override bool CanWrite => (access & FileAccess.Write) == FileAccess.Write;

        private void* OpenFile(FileMode mode)
        {
            // TODO: the special cases are not thread-safe and certainly
            // not atomic. How can we handle this?

            // Handle special cases first. These special cases call
            // `OpenFile` recursively, but the `FileMode` they pass is
            // never a special case---so we won't have infinite recusion
            // here.
            if (mode == FileMode.Truncate)
            {
                if (!FileExists(Name))
                {
                    throw new IOException(
                        "Cannot truncate file at '" + Name +
                        "' because it does not exist.");
                }
                return OpenFile(FileMode.Open);
            }
            else if (mode == FileMode.CreateNew)
            {
                if (FileExists(Name))
                {
                    throw new IOException(
                        "Cannot create a new file at '" + Name +
                        "' as one already exists.");
                }
                return OpenFile(FileMode.Create);
            }
            else if (mode == FileMode.OpenOrCreate)
            {
                return OpenFile(FileExists(Name) ? FileMode.Open : FileMode.Create);
            }

            // Handle basic open/create/append operations here.
            switch (mode)
            {
                case FileMode.Open:
                    return OpenFile(Name, CanWrite ? "r+b" : "rb");

                case FileMode.Create:
                    return OpenFile(Name, CanRead ? "w+b" : "wb");

                case FileMode.Append:
                    return OpenFile(Name, CanRead ? "a+b" : "ab");

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
            if (handle == (void*)null)
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
            CheckReadArgs(buffer, offset, count);
            return (int)IOPrimitives.ReadFromFile(&buffer[offset], (ulong)count, fileHandle);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckReadArgs(buffer, offset, count);
            ulong bytesWritten = IOPrimitives.WriteToFile(&buffer[offset], (ulong)count, fileHandle);
            if (bytesWritten < (ulong)count)
            {
                throw new IOException("Wrote only " + bytesWritten + " of " + count + " bytes.");
            }
        }
    }
}