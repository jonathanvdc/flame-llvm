namespace System.IO
{
    /// <summary>
    /// Defines input/output primitives, which can be used to 
    /// </summary>
    public static class IOPrimitives
    {
        private extern static int fgetc(void* file);
        private extern static int fputc(int ch, void* file);
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
