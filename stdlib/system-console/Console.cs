// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

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

        unroll ((TYPE) in (bool, int, long, double))
        {
            /// <summary>
            /// Writes the given value to standard output.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void Write(TYPE value)
            {
                Write(value.ToString());
            }

            /// <summary>
            /// Writes the given value to standard output, followed by an end-of-line sequence.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void WriteLine(TYPE value)
            {
                Write(value);
                WriteLine();
            }
        }
    }
}