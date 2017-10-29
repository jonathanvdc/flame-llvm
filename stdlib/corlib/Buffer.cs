using System.Primitives.InteropServices;

namespace System
{
    public static unsafe class Buffer
    {
        /// <summary>
        /// Copies a number of bytes from one address in memory to another.
        /// </summary>
        /// <param name="source">
        /// The address of the source buffer.
        /// </param>
        /// <param name="destination">
        /// The address of the destination buffer.
        /// </param>
        /// <param name="destinationSizeInBytes">
        /// The size of the destination buffer, in bytes.
        /// </param>
        /// <param name="sourceBytesToCopy">
        /// The number of bytes to copy from the source buffer.
        /// </param>
        public static void MemoryCopy(
            void* source,
            void* destination,
            ulong destinationSizeInBytes,
            ulong sourceBytesToCopy)
        {
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "sourceBytesToCopy",
                    sourceBytesToCopy,
                    "sourceBytesToCopy is greater than destinationSizeInBytes (" +
                    destinationSizeInBytes + ").");
            }

            Memory.MemoryMove(source, destination, sourceBytesToCopy);
        }

        /// <summary>
        /// Copies a number of bytes from one address in memory to another.
        /// </summary>
        /// <param name="source">
        /// The address of the source buffer.
        /// </param>
        /// <param name="destination">
        /// The address of the destination buffer.
        /// </param>
        /// <param name="destinationSizeInBytes">
        /// The size of the destination buffer, in bytes.
        /// </param>
        /// <param name="sourceBytesToCopy">
        /// The number of bytes to copy from the source buffer.
        /// </param>
        public static void MemoryCopy(
            void* source,
            void* destination,
            long destinationSizeInBytes,
            long sourceBytesToCopy)
        {
            MemoryCopy(source, destination, (ulong)destinationSizeInBytes, (ulong)sourceBytesToCopy);
        }

        #region Non-standard overloads of BlockCopy

        public static void BlockCopy(
            byte[] source,
            int sourcePos,
            byte[] destination,
            int destinationPos,
            int count)
        {
            BlockCopy(&source[sourcePos], &destination[destinationPos], count);
        }

        public static void BlockCopy(
            byte* source,
            byte* destination,
            int count)
        {
            Memory.MemoryMove(source, destination, (ulong)count);
        }

        public static void BlockCopy(
            byte* source,
            byte[] destination,
            int destinationPos,
            int count)
        {
            BlockCopy(source, &destination[destinationPos], count);
        }

        public static void BlockCopy(
            byte[] source,
            int sourcePos,
            byte* destination,
            int count)
        {
            BlockCopy(&source[sourcePos], destination, count);
        }

        #endregion
    }
}