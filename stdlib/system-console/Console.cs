// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

using System.IO;

namespace System
{
    public static class Console
    {
        private static readonly byte[] utf8Buffer = new byte[4];
        private static readonly char[] utf16Buffer = new char[2];
        private static char cachedHighSurrogate = '\0';

        private static void WriteUtf8Bytes(byte[] str, int count)
        {
            for (int i = 0; i < count; i++)
            {
                IOPrimitives.WriteStdout(str[i]);
            }
        }

        private static void WriteUtf16CodePoint(char* begin, char* end)
        {
            int byteCount = UnicodeConverter.WriteUtf8CodePoint(
                UnicodeConverter.ReadUtf16CodePoint(ref begin, end),
                &utf8Buffer[0]);

            WriteUtf8Bytes(utf8Buffer, byteCount);
        }

        /// <summary>
        /// Writes a character to standard output.
        /// </summary>
        /// <param name="c">The character to write.</param>
        public static void Write(char c)
        {
            char highSurrogate = cachedHighSurrogate;
            if (highSurrogate == '\0')
            {
                if (char.IsHighSurrogate(c))
                {
                    cachedHighSurrogate = c;
                }
                else
                {
                    char* argBegin = &c;
                    WriteUtf16CodePoint(argBegin, argBegin + 1);
                }
            }
            else
            {
                cachedHighSurrogate = '\0';
                utf16Buffer[0] = highSurrogate;
                utf16Buffer[1] = c;
                char* argBegin = &utf16Buffer[0];
                WriteUtf16CodePoint(argBegin, argBegin + 2);
            }
        }

        public static void Flush()
        {
            char highSurrogate = cachedHighSurrogate;
            if (highSurrogate != '\0')
            {
                cachedHighSurrogate = '\0';
                char* argBegin = &highSurrogate;
                WriteUtf16CodePoint(argBegin, argBegin + 1);
            }
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