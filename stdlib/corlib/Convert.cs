// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

namespace System
{
    /// <summary>
    /// Converts between primitive types.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts a signed 64-bit signed integer to a string.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>A string representation for the integer.</returns>
        public static string ToString(long value)
        {
            // Based on the code from bhuwansahni's answer to this StackOverflow question:
            // https://stackoverflow.com/questions/9655202/how-to-convert-integer-to-string-in-c

            int length = 0;
            bool isNegative = false;
            if (value < 0)
            {
                length++;
                isNegative = true;
                value = -value;
            }

            long shifter = value;
            do
            {
                // Move to where the representation ends.
                length++;
                shifter /= 10;
            } while (shifter != 0);

            var array = new char[length];
            int offset = length - 1;
            do
            {
                // Move back, inserting digits as we go.
                array[offset] = (char)(value % 10 + (int)'0');
                offset--;
                value /= 10;
            } while (value != 0);

            if (isNegative)
            {
                array[0] = '-';
            }

            return new String(array);
        }

        /// <summary>
        /// Converts an unsigned 64-bit signed integer to a string.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>A string representation for the integer.</returns>
        public static string ToString(ulong value)
        {
            // Based on the code from bhuwansahni's answer to this StackOverflow question:
            // https://stackoverflow.com/questions/9655202/how-to-convert-integer-to-string-in-c

            int length = 0;

            ulong shifter = value;
            do
            {
                // Move to where the representation ends.
                length++;
                shifter /= 10;
            } while (shifter != 0);

            var array = new char[length];
            int offset = length - 1;
            do
            {
                // Move back, inserting digits as we go.
                array[offset] = (char)(value % 10 + (int)'0');
                offset--;
                value /= 10;
            } while (value != 0);

            return new String(array);
        }

        unroll ((TYPE) in (sbyte, short, int))
        {
            /// <summary>
            /// Converts the given signed integer value to a string.
            /// </summary>
            /// <param name="value">The integer value.</param>
            /// <returns>A string representation for the integer.</returns>
            public static string ToString(TYPE value)
            {
                return ToString((long)value);
            }
        }

        unroll ((TYPE) in (byte, ushort, uint))
        {
            /// <summary>
            /// Converts the given unsigned integer value to a string.
            /// </summary>
            /// <param name="value">The integer value.</param>
            /// <returns>A string representation for the integer.</returns>
            public static string ToString(TYPE value)
            {
                return ToString((ulong)value);
            }
        }

        /// <summary>
        /// Converts the given 64-bit floating-point number to a string.
        /// </summary>
        /// <param name="value">The floating-point number.</param>
        /// <returns>A string representation for the floating-point number.</returns>
        public static string ToString(double value)
        {
            // FIXME: actually implement this function
            return ToString((long)value);
        }

        /// <summary>
        /// Converts the given Boolean to a string.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        /// <returns>A string representation for the Boolean.</returns>
        public static string ToString(bool value)
        {
            return value ? "True" : "False";
        }
    }
}