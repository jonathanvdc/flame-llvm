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

        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static int AtomicRMWAdd(
            ref int destination,
            int value);

        /// <summary>
        /// Adds two 32-bit signed integers and replaces the first
        /// with the sum, in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read from
        /// and then written to in an atomic operation.
        /// </param>
        /// <param name="value">
        /// A value to add to the destination value.
        /// </param>
        /// <returns>
        /// The new value at the destination.
        /// </returns>
        public static int Add(
            ref int destination,
            int value)
        {
            return AtomicRMWAdd(ref destination, value);
        }

        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static long AtomicRMWAdd(
            ref long destination,
            long value);

        /// <summary>
        /// Adds two 64-bit signed integers and replaces the first
        /// with the sum, in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read from
        /// and then written to in an atomic operation.
        /// </param>
        /// <param name="value">
        /// A value to add to the destination value.
        /// </param>
        /// <returns>
        /// The new value at the destination.
        /// </returns>
        public static long Add(
            ref long destination,
            long value)
        {
            return AtomicRMWAdd(ref destination, value);
        }

        /// <summary>
        /// Increments a 32-bit signed integer at a particular destination,
        /// in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read and then written to
        /// in an atomic operation.
        /// </param>
        /// <returns>The new value at the destination.</returns>
        public static int Increment(ref int destination)
        {
            return Add(ref destination, 1);
        }

        /// <summary>
        /// Increments a 64-bit signed integer at a particular destination,
        /// in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read and then written to
        /// in an atomic operation.
        /// </param>
        /// <returns>The new value at the destination.</returns>
        public static long Increment(ref long destination)
        {
            return Add(ref destination, 1);
        }

        /// <summary>
        /// Decrements a 32-bit signed integer at a particular destination,
        /// in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read and then written to
        /// in an atomic operation.
        /// </param>
        /// <returns>The new value at the destination.</returns>
        public static int Decrement(ref int destination)
        {
            return Add(ref destination, -1);
        }

        /// <summary>
        /// Decrements a 64-bit signed integer at a particular destination,
        /// in an atomic operation.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination, whose value is first read and then written to
        /// in an atomic operation.
        /// </param>
        /// <returns>The new value at the destination.</returns>
        public static long Decrement(ref long destination)
        {
            return Add(ref destination, -1);
        }
    }
}