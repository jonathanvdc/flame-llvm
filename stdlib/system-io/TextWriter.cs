// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

using System.Text;

namespace System.IO
{
    /// <summary>
    /// A writer that writes sequences of characters.
    /// </summary>
    public abstract class TextWriter : IDisposable
    {
        protected TextWriter()
        {
            CoreNewLineStr = Environment.NewLine;
            CoreNewLine = CoreNewLineStr.ToCharArray();
        }

        protected char[] CoreNewLine;
        private string CoreNewLineStr;

        /// <summary>
        /// Gets the encoding used by this text writer.
        /// </summary>
        /// <returns>The encoding.</returns>
        public abstract Encoding Encoding { get; }

        public virtual string NewLine
        {
            get { return CoreNewLineStr; }
            set
            {
                if (value == null)
                {
                    value = Environment.NewLine;
                }

                CoreNewLineStr = value;
                CoreNewLine = value.ToCharArray();
            }
        }

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
        public virtual void Flush() { }

        public static readonly TextWriter Null;
        public static TextWriter Synchronized(TextWriter writer) { throw null; }

        /// <summary>
        /// Prints out a character.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public virtual void Write(char value)
        {
            // Do nothing. This is our "root" function. Ideally,
            // this would've been abstract.
        }

        /// <summary>
        /// Prints out a character, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public virtual void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Prints out the characters in a buffer.
        /// </summary>
        /// <param name="buffer">A buffer containing the characters to write.</param>
        public virtual void Write(char[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Prints out the characters in a buffer, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="buffer">A buffer containing the characters to write.</param>
        public virtual void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        /// <summary>
        /// Prints out a range of characters in a buffer.
        /// </summary>
        /// <param name="buffer">A buffer containing the characters to write.</param>
        /// <param name="index">The index of the first character to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public virtual void Write(char[] buffer, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Write(buffer[index + i]);
            }
        }

        /// <summary>
        /// Prints out a range of characters in a buffer, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="buffer">A buffer containing the characters to write.</param>
        /// <param name="index">The index of the first character to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public virtual void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        /// <summary>
        /// Prints out a string.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public virtual void Write(string value)
        {
            if (value != null)
            {
                Write(value.ToCharArray());
            }
        }

        /// <summary>
        /// Prints out a string, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public virtual void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Prints out an object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public virtual void Write(object value)
        {
            if (value != null)
            {
                Write(value.ToString());
            }
        }

        /// <summary>
        /// Prints out an object, followed by an end-of-line sequence.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public virtual void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        unroll ((TYPE) in (
            bool,
            sbyte, byte, short, ushort, int, uint, long, ulong,
            float, double))
        {
            /// <summary>
            /// Prints out a value.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public virtual void Write(TYPE value)
            {
                Write(value.ToString());
            }

            /// <summary>
            /// Prints out a value, followed by an end-of-line sequence.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public virtual void WriteLine(TYPE value)
            {
                Write(value);
                WriteLine();
            }
        }

        /// <summary>
        /// Prints out an end-of-line sequence.
        /// </summary>
        public virtual void WriteLine()
        {
            Write(CoreNewLine);
        }
    }
}