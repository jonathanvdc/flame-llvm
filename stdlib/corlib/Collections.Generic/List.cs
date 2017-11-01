namespace System.Collections.Generic
{
    /// <summary>
    /// A strongly-typed array list.
    /// </summary>
    public class List<T> : Object, IEnumerable<T>
    {
        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public List()
        {
            this.contents = new T[initialCapacity];
            this.size = 0;
        }

        private T[] contents;
        private int size;

        private const int initialCapacity = 4;

        /// <summary>
        /// Gets the list's capacity.
        /// </summary>
        public int Capacity => contents.Length;

        /// <summary>
        /// Gets the list's current size.
        /// </summary>
        public int Count => size;

        /// <summary>
        /// Gets or sets the nth element in this list.
        /// </summary>
        public T this[int index]
        {
            get
            {
                return contents[index];
            }
            set
            {
                contents[index] = value;
            }
        }

        /// <summary>
        /// Adds the given value at back of this list.
        /// </summary>
        /// <param name="value">The value to add to the list.</param>
        public void Add(T value)
        {
            EnsureCapacity(size + 1);

            contents[size] = value;
            size++;
        }

        /// <summary>
        /// Ensures that the capacity of this list is at least the given value.
        /// </summary>
        /// <param name="minCapacity">The min capacity for this list.</param>
        private void EnsureCapacity(int minCapacity)
        {
            if (Capacity < minCapacity)
            {
                int newCapacity = Math.Max(
                    Math.Max(initialCapacity, 2 * Capacity),
                    minCapacity);
                ResizeCapacity(newCapacity);
            }
        }

        /// <summary>
        /// Resizes this list to the given capacity.
        /// </summary>
        /// <param name="newCapacity">The new capacity.</param>
        private void ResizeCapacity(int newCapacity)
        {
            var newContents = new T[newCapacity];
            for (int i = 0; i < size; i++)
            {
                newContents[i] = contents[i];
            }
            contents = newContents;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(contents, 0, size);
        }
    }

    internal sealed class ArrayEnumerator<T> : IEnumerator<T>
    {
        public ArrayEnumerator(T[] array, int startIndex, int count)
        {
            this.array = array;
            this.startIndex = startIndex;
            this.endIndex = startIndex + count;
            this.currentIndex = startIndex;
        }

        private T[] array;
        private int startIndex;
        private int endIndex;
        private int currentIndex;

        private bool IsInBounds => currentIndex < endIndex;

        public T Current
        {
            get
            {
                if (array == null)
                    throw new ObjectDisposedException("this");
                else if (IsInBounds)
                    return array[currentIndex];
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public bool MoveNext()
        {
            if (array == null)
                throw new ObjectDisposedException("this");

            currentIndex++;
            return IsInBounds;
        }

        public void Dispose()
        {
            array = null;
        }

        public void Reset()
        {
            if (array == null)
                throw new ObjectDisposedException("this");

            currentIndex = startIndex;
        }
    }
}