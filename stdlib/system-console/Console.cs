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
            stderr = new FileStream(
                (IntPtr)IOPrimitives.StandardErrorFile,
                FileAccess.Write);

            stdout = new FileStream(
                (IntPtr)IOPrimitives.StandardOutputFile,
                FileAccess.Write);

            Error = TextWriter.Synchronized(new StreamWriter(stderr, Encoding.UTF8));
            Out = TextWriter.Synchronized(new StreamWriter(stdout, Encoding.UTF8));
        }

        private static readonly FileStream stderr;
        private static readonly FileStream stdout; 

        /// <summary>
        /// Gets the standard error text writer.
        /// </summary>
        /// <returns>The standard error text writer.</returns>
        public static TextWriter Error { get; private set; }

        /// <summary>
        /// Gets the standard output text writer.
        /// </summary>
        /// <returns>The standard output text writer.</returns>
        public static TextWriter Out { get; private set; }

        /// <summary>
        /// Sets the standard error text writer.
        /// </summary>
        /// <param name="writer">A new error text writer.</param>
        public static void SetError(TextWriter writer)
        {
            Error = writer;
        }

        /// <summary>
        /// Sets the standard output text writer.
        /// </summary>
        /// <param name="writer">A new output text writer.</param>
        public static void SetOut(TextWriter writer)
        {
            Out = writer;
        }

        /// <summary>
        /// Acquires the standard error stream.
        /// </summary>
        /// <returns>The standard error stream.</returns>
        public Stream OpenStandardError()
        {
            return stderr;
        }

        /// <summary>
        /// Acquires the standard output stream.
        /// </summary>
        /// <returns>The standard output stream.</returns>
        public Stream OpenStandardOutput()
        {
            return stdout;
        }

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
            /// Writes a value to standard output.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void Write(TYPE value)
            {
                Out.Write(value.ToString());
            }

            /// <summary>
            /// Writes a value to standard output, followed by an end-of-line sequence.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public static void WriteLine(TYPE value)
            {
                Out.WriteLine(value);
            }
        }
    }
}