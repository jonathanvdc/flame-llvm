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
        public static void Write(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Write(str[i]);
            }
        }

        /// <summary>
        /// Writes an integer to standard output.
        /// </summary>
        /// <param name="i">The integer to write.</param>
        public static void Write(int i)
        {
            Write(i.ToString());
        }

        /// <summary>
        /// Writes the given value to standard output.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public static void Write(double value)
        {
            Write(Convert.ToString(value));
        }

        /// <summary>
        /// Writes a Boolean to standard output.
        /// </summary>
        /// <param name="value">The Boolean to write.</param>
        public static void Write(bool value)
        {
            Write(value.ToString());
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
        public static void WriteLine(string str)
        {
            Write(str);
            WriteLine();
        }

        /// <summary>
        /// Writes an integer to standard output, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="i">The integer to write.</param>
        public static void WriteLine(int i)
        {
            Write(i);
            WriteLine();
        }

        /// <summary>
        /// Writes a Boolean to standard output, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="value">The Boolean to write.</param>
        public static void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }
    }
}