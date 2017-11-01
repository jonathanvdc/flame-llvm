#importMacros(LeMP.CSharp6);

namespace System
{
    // TODO: tweak the compiler to make all arrays implement 'System.Array'.
    public abstract class Array : Object
    {
        public static void Clear(byte[] array, int index, int length)
        {
            Clear<byte>(array, index, length);
        }

        public static void Clear<T>(T[] array, int index, int length)
        {
            EnsureValidRange(
                nameof(index), nameof(array),
                index, length, array.Length);

            for (int i = 0; i < length; i++)
            {
                array[index + i] = default(T);
            }
        }

        public static unsafe void Copy<T>(
            T[] source, int sourceIndex,
            T[] destination, int destinationIndex,
            int length)
        {
            if (length == 0)
                return;

            EnsureValidRange(
                nameof(sourceIndex), nameof(source),
                sourceIndex, length, source.Length);

            EnsureValidRange(
                nameof(destinationIndex), nameof(destination),
                destinationIndex, length, destination.Length);

            Buffer.MemoryCopy(
                &source[sourceIndex],
                &destination[destinationIndex],
                (destination.Length - destinationIndex) * sizeof(T),
                length * sizeof(T));
        }

        internal static bool IsValidRange(int rangeStart, int rangeLength, int dataLength)
        {
            return rangeStart + rangeLength <= dataLength;
        }

        internal static void EnsureValidRange(
            string argName, string arrayName,
            int rangeStart, int rangeLength, int arrayLength)
        {
            EnsureInArray(argName, arrayName, rangeStart, arrayLength);

            if (!IsValidRange(rangeStart, rangeLength, arrayLength))
            {
                throw new ArgumentOutOfRangeException(
                    argName,
                    rangeStart,
                    arrayName + " array range [" + rangeStart +
                    ", " + (rangeStart + rangeLength) + ") is out of bounds.");
            }
        }

        internal static void EnsureInArray(
            string argName, string arrayName,
            int argValue, int arrayLength)
        {
            if (argValue >= arrayLength)
            {
                throw new ArgumentOutOfRangeException(
                    argName,
                    argValue,
                    argName + " is greater than the length of the " + arrayName + " array.");
            }
        }
    }
}