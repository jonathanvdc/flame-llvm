#importMacros(LeMP);

namespace System.Threading
{
    /// <summary>
    /// Implements atomic operations.
    /// </summary>
    public static class Interlocked
    {
        unroll ((TYPE) in (int, long, IntPtr, object))
        {
            /// <summary>
            /// Compares two values for equality and,
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
            public static TYPE CompareExchange(
                ref TYPE destination,
                TYPE value,
                TYPE comparand);

            [#builtin_attribute(RuntimeImplementedAttribute)]
            private static TYPE AtomicRMWXchg(
                ref TYPE destination,
                TYPE value);

            /// <summary>
            /// Atomically replaces the value at a particular destination.
            /// </summary>
            /// <param name="destination">
            /// A reference to a destination whose value is replaced.
            /// </param>
            /// <param name="value">
            /// A new value to store in the destination.
            /// </param>
            /// <returns>
            /// The old value in the destination.
            /// </returns>
            public static TYPE Exchange(
                ref TYPE destination,
                TYPE value)
            {
                return AtomicRMWXchg(ref destination, value);
            }
        }

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
        private static T AtomicRMWXchg<T>(
            ref T destination,
            T value)
            where T : class;

        /// <summary>
        /// Atomically replaces the value at a particular destination.
        /// </summary>
        /// <param name="destination">
        /// A reference to a destination whose value is replaced.
        /// </param>
        /// <param name="value">
        /// A new value to store in the destination.
        /// </param>
        /// <returns>
        /// The old value in the destination.
        /// </returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        public static T Exchange<T>(
            ref T destination,
            T value)
            where T : class
        {
            return AtomicRMWXchg<T>(ref destination, value);
        }

        unroll ((TYPE) in (int, long))
        {
            [#builtin_attribute(RuntimeImplementedAttribute)]
            private static int AtomicRMWAdd(
                ref TYPE destination,
                TYPE value);

            /// <summary>
            /// Adds two values and replaces the first
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
            public static TYPE Add(
                ref TYPE destination,
                TYPE value)
            {
                return AtomicRMWAdd(ref destination, value);
            }

            /// <summary>
            /// Increments a value at a particular destination,
            /// in an atomic operation.
            /// </summary>
            /// <param name="destination">
            /// A reference to a destination, whose value is first
            /// read and then written to in an atomic operation.
            /// </param>
            /// <returns>The new value at the destination.</returns>
            public static TYPE Increment(ref TYPE destination)
            {
                return Add(ref destination, 1);
            }

            /// <summary>
            /// Decrements a value at a particular destination,
            /// in an atomic operation.
            /// </summary>
            /// <param name="destination">
            /// A reference to a destination, whose value is first
            /// read and then written to in an atomic operation.
            /// </param>
            /// <returns>The new value at the destination.</returns>
            public static TYPE Decrement(ref TYPE destination)
            {
                return Add(ref destination, -1);
            }
        }
    }
}