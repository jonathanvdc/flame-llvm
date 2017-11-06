namespace System.Threading
{
    /// <summary>
    /// Implements atomic operations.
    /// </summary>
    public static class Interlocked
    {
        /// <summary>
        /// Compares two 32-bit signed integers for equality and,
        /// if they are equal, replaces the first value.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </param>
        /// <param name="value">
        /// A value that replaces the destination value if the comparison
        /// results in equality.
        /// </param>
        /// <param name="comparand">
        /// A value that is compared to the value at the destination.
        /// </param>
        /// <returns>
        /// The original value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static int CompareExchange(
            ref int destination,
            int value,
            int comparand);

        /// <summary>
        /// Compares two 64-bit signed integers for equality and,
        /// if they are equal, replaces the first value.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </param>
        /// <param name="value">
        /// A value that replaces the destination value if the comparison
        /// results in equality.
        /// </param>
        /// <param name="comparand">
        /// A value that is compared to the value at the destination.
        /// </param>
        /// <returns>
        /// The original value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static long CompareExchange(
            ref long destination,
            long value,
            long comparand);

        /// <summary>
        /// Compares two pointers for equality and,
        /// if they are equal, replaces the first value.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </param>
        /// <param name="value">
        /// A value that replaces the destination value if the comparison
        /// results in equality.
        /// </param>
        /// <param name="comparand">
        /// A value that is compared to the value at the destination.
        /// </param>
        /// <returns>
        /// The original value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static IntPtr CompareExchange(
            ref IntPtr destination,
            IntPtr value,
            IntPtr comparand);

        /// <summary>
        /// Compares two references for equality and,
        /// if they are equal, replaces the first value.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </param>
        /// <param name="value">
        /// A value that replaces the destination value if the comparison
        /// results in equality.
        /// </param>
        /// <param name="comparand">
        /// A value that is compared to the value at the destination.
        /// </param>
        /// <returns>
        /// The original value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static object CompareExchange(
            ref object destination,
            object value,
            object comparand);

        /// <summary>
        /// Compares two references for equality and,
        /// if they are equal, replaces the first value.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </param>
        /// <param name="value">
        /// A value that replaces the destination value if the comparison
        /// results in equality.
        /// </param>
        /// <param name="comparand">
        /// A value that is compared to the value at the destination.
        /// </param>
        /// <returns>
        /// The original value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static T CompareExchange<T>(
            ref T destination,
            T value,
            T comparand)
            where T : class;
    }
}