using System.IO;

namespace System
{
    public static class Console
    {
        /// <summary>
        /// Writes a character to standard output.
        /// </summary>
        /// <param name="c">The character to write.</param>
        public static void Write(char c)
        {
            // TODO: convert this character to UTF-8 before writing it.
            IOPrimitives.WriteStdout((byte)c);
        }

        /// <summary>
        /// Writes a string to standard output.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public static void Write(String str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Write(str[i]);
            }
        }

        /// <summary>
        /// Writes an end-of-line sequence to standard output.
        /// </summary>
        public static void WriteLine()
        {
            Write('\n');
        }

        /// <summary>
        /// Writes a character to standard output, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="c">The character to write.</param>
        public static void WriteLine(char c)
        {
            Write(c);
            WriteLine();
        }

        /// <summary>
        /// Writes a string to standard output, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public static void WriteLine(String str)
        {
            Write(str);
            WriteLine();
        }
    }
}