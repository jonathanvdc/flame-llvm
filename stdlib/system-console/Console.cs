// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

using System.Primitives.IO;
using System.Primitives.Text;
using System.IO;
using System.Text;

namespace System
{
    public static class Console
    {
        static Console()
        {
            Error = new StreamWriter(
                new FileStream((IntPtr)IOPrimitives.StandardErrorFile, FileAccess.Write),
                Encoding.UTF8);

            Out = new StreamWriter(
                new FileStream((IntPtr)IOPrimitives.StandardOutputFile, FileAccess.Write),
                Encoding.UTF8);
        }

        /// <summary>
        /// Gets or sets the standard error text writer.
        /// </summary>
        /// <returns>The standard error text writer.</returns>
        public static TextWriter Error { get; set; }

        /// <summary>
        /// Gets or sets the standard output text writer.
        /// </summary>
        /// <returns>The standard output text writer.</returns>
        public static TextWriter Out { get; set; }

        /// <summary>
        /// Flushes the standard output buffer.
        /// </summary>
        public static void Flush()
        {
            Out.Flush();
        }

        /// <summary>
        /// Writes an end-of-line sequence to standard output.
        /// </summary>
        public static void WriteLine()
        {
            Out.WriteLine();
        }

        unroll ((TYPE) in (
            char, string, bool, object,
            sbyte, short, int, long,
            byte, ushort, uint, ulong,
            float, double))
        {
            /// <summary>
            /// Writes the given value to standard output.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void Write(TYPE value)
            {
                Out.Write(value.ToString());
            }

            /// <summary>
            /// Writes the given value to standard output, followed by an end-of-line sequence.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void WriteLine(TYPE value)
            {
                Out.WriteLine(value);
            }
        }
    }
}