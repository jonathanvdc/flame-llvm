namespace System
{
    /// <summary>
    /// Converts between data types based on their underlying storage.
    /// </summary>
    public static class BitConverter
    {
        /// <summary>
        /// Reinterprets a double-precision floating-point number's underlying storage
        /// as a 64-bit integer.
        /// </summary>
        /// <param name="value">A double-precision floating-point number.</param>
        /// <returns>A 64-bit integer.</returns>
        public static unsafe long DoubleToInt64Bits(double value)
        {
            var dataPtr = &value;
            return *(long*)dataPtr;
        }

        /// <summary>
        /// Reinterprets a 64-bit integer's underlying storage as a double-precision
        /// floating-point number.
        /// </summary>
        /// <param name="value">64-bit integer.</param>
        /// <returns>A double-precision floating-point number.</returns>
        public static unsafe double Int64BitsToDouble(long value)
        {
            var dataPtr = &value;
            return *(double*)dataPtr;
        }

        /// <summary>
        /// Reinterprets a single-precision floating-point number's underlying storage
        /// as a 32-bit integer.
        /// </summary>
        /// <param name="value">A single-precision floating-point number.</param>
        /// <returns>A 32-bit integer.</returns>
        public static unsafe int SingleToInt32Bits(float value)
        {
            var dataPtr = &value;
            return *(int*)dataPtr;
        }

        /// <summary>
        /// Reinterprets a 32-bit integer's underlying storage as a single-precision
        /// floating-point number.
        /// </summary>
        /// <param name="value">32-bit integer.</param>
        /// <returns>A single-precision floating-point number.</returns>
        public static unsafe float Int32BitsToSingle(int value)
        {
            var dataPtr = &value;
            return *(float*)dataPtr;
        }
    }
}