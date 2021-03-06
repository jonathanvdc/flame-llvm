using System.Collections.Generic;

namespace System
{
    // TODO: ArraySegment<T> is not feature-complete yet. It lacks
    // - Equals/GetHashCode overrides
    // - ICollection<T>, IList<T>, ... implementations

    /// <summary>
    /// Describes a segment of an array.
    /// </summary>
    public struct ArraySegment<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// Creates an array segment that encapsulates an array
        /// in its entirety.
        /// </summary>
        /// <param name="data">An array.</param>
        public ArraySegment(T[] data)
        {
            this = default(ArraySegment<T>);
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Array = data;
            Offset = 0;
            Count = data.Length;
        }

        /// <summary>
        /// Creates an array segment.
        /// </summary>
        /// <param name="data">An array.</param>
        /// <param name="offset">A start offset in the array.</param>
        /// <param name="count">The number of items to select in the array.</param>
        public ArraySegment(T[] data, int offset, int count)
        {
            this = default(ArraySegment<T>);
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            else if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    offset,
                    "offset is less than zero.");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    "count is less than zero.");
            }
            else if (offset + count > data.Length)
            {
                throw new ArgumentException(
                    nameof(count),
                    "offset + count is greater than the length of the array.");
            }

            Array = data;
            Offset = offset;
            Count = count;
        }

        /// <summary>
        /// Gets the original array containing the range of elements
        /// that the array delimits.
        /// </summary>
        /// <returns>The original array.</returns>
        public T[] Array { get; private set; }

        /// <summary>
        /// Gets the position of the first element in the range
        /// delimited by the array segment, relative to the start of
        /// the original array.
        /// </summary>
        /// <returns>
        /// The position in the array of the first element in the
        /// array segment.</returns>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the number of elements in the range delimited by the
        /// array segment.
        /// </summary>
        /// <returns>The number of elements in the array segment.</returns>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the element at a particular index in this array segment.
        /// </summary>
        public T this[int index]
        {
            get
            {
                EnsureInBounds(index);
                return Array[Offset + index];
            }
            set
            {
                EnsureInBounds(index);
                Array[Offset + index] = value;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(Array, Offset, Count);
        }

        private void EnsureInBounds(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}