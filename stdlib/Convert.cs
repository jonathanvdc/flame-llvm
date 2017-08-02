namespace System
{
    /// <summary>
    /// Converts between primitive types.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts the given 32-bit signed integer to a string.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>A string representation for the integer.</returns>
        public static string ToString(int value)
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

            int shifter = value;
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