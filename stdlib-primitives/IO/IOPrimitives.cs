namespace System.Primitives.IO
{
    /// <summary>
    /// Defines input/output primitives, which can be used to 
    /// </summary>
    public static class IOPrimitives
    {
        private extern static int fgetc(void* file);
        private extern static int fputc(int ch, void* file);

        private extern static void* fopen(byte* name, byte* mode);
        private extern static void fclose(void* stream);
        private extern static void fflush(void* stream);
        private extern static size_t fread(void* buffer, size_t size, size_t count, void* stream);
        private extern static size_t fwrite(void* buffer, size_t size, size_t count, void* stream);
        private extern static long ftell(void* stream);
        private extern static int fseek(void* stream, long offset, int origin);

        /// <summary>
        /// Opens the file with the specified name in the given mode.
        /// </summary>
        /// <param name="name">The name of the file to open.</param>
        /// <param name="mode">The mode to open the file in.</param>
        public static void* OpenFile(byte* name, byte* mode)
        {
            return fopen(name, mode);
        }

        /// <summary>
        /// Closes the given file.
        /// </summary>
        /// <param name="stream">A file handle.</param>
        public static void CloseFile(void* stream)
        {
            fclose(stream);
        }

        /// <summary>
        /// Synchronizes an output stream with the underlying file.
        /// </summary>
        /// <param name="stream">The output stream to synchronize with a file.</param>
        public static void FlushFile(void* stream)
        {
            fflush(stream);
        }

        /// <summary>
        /// Gets the current position in the given file.
        /// </summary>
        /// <param name="stream">A file handle.</param>
        /// <returns>The position in the file.</returns>
        public static long GetFilePosition(void* stream)
        {
            return ftell(stream);
        }

        /// <summary>
        /// Seeks to the specified offset from an origin in the given file. 
        /// </summary>
        /// <param name="stream">A file handle.</param>
        /// <param name="offset">An offset.</param>
        /// <param name="origin">An origin to seek from.</param>
        /// <returns></returns>
        public static int FileSeek(void* stream, long offset, int origin)
        {
            return fseek(stream, offset, origin);
        }

        /// <summary>
        /// Reads a given number of bytes from a file into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="stream">The file to read the bytes from.</param>
        /// <returns>The number of bytes that were read.</returns>
        public static ulong ReadFromFile(byte* buffer, ulong count, void* stream)
        {
            return (ulong)fread(buffer, (size_t)1u, (size_t)count, stream);
        }

        /// <summary>
        /// Writes a given number of bytes from a buffer to a file.
        /// </summary>
        /// <param name="buffer">The buffer to read the bytes from.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="stream">The file to write the bytes to.</param>
        /// <returns>The number of bytes that were written.</returns>
        public static ulong WriteToFile(byte* buffer, ulong count, void* stream)
        {
            return (ulong)fwrite(buffer, (size_t)1u, (size_t)count, stream);
        }

        private extern static void* stdin;
        private extern static void* stdout;
        private extern static void* stderr;

        /// <summary>
        /// Writes a single byte to standard output.
        /// </summary>
        /// <param name="b">The byte to write.</param>
        /// <returns><c>true</c> if nothing went wrong; otherwise, <c>false</c>.</returns>
        public static bool WriteStdout(byte b)
        {
            return fputc(b, stdout) >= 0;
        }

        /// <summary>
        /// Writes a single byte to standard error.
        /// </summary>
        /// <param name="b">The byte to write.</param>
        /// <returns><c>true</c> if nothing went wrong; otherwise, <c>false</c>.</returns>
        public static bool WriteStderr(byte b)
        {
            return fputc(b, stderr) >= 0;
        }

        /// <summary>
        /// Reads a single byte from standard input.
        /// </summary>
        /// <returns>
        /// The byte that was read. A negative value means that something went wrong.
        /// </returns>
        public static int ReadStdin()
        {
            return fgetc(stdin);
        }
    }
}
