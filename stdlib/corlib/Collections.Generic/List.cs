namespace System.Collections.Generic
{
    /// <summary>
    /// A strongly-typed array list.
    /// </summary>
    public class List<T> : Object, IList<T>, IReadOnlyList<T>
    {
        /// <summary>
        /// Creates an empty list.
        /// </summary>
        public List()
        {
            Clear();
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

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the element at a particular index in this list.
        /// </summary>
        public T this[int index]
        {
            get
            {
                EnsureInBounds(index);
                return contents[index];
            }
            set
            {
                EnsureInBounds(index);
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
        /// Checks if the list contains a value.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>
        /// <c>true</c> if the list contains the value; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T value)
        {
            return IndexOf(value) >= 0;
        }

        /// <summary>
        /// Gets the index in the list of the first occurrence of a value.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>
        /// The index of the first occurrence of a value, if it is in the list;
        /// otherwise, a negative value.
        /// </returns>
        public int IndexOf(T value)
        {
            for (int i = 0; i < size; i++)
            {
                if (object.Equals(contents[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Copies the contents of the list to an array, starting
        /// at an offset in the array.
        /// </summary>
        /// <param name="destination">The destination array.</param>
        /// <param name="offset">
        /// The offset where the first element of this list is placed.
        /// </param>
        public unsafe void CopyTo(T[] destination, int offset)
        {
            if (destination.Length - offset < Count)
            {
                throw new ArgumentException(
                    nameof(destination),
                    "Destination array is not large enough.");
            }

            Array.Copy<T>(contents, 0, destination, offset, size);
            // for (int i = 0; i < size; i++)
            // {
            //     destination[offset + i] = contents[i];
            // }
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            this.contents = new T[initialCapacity];
            this.size = 0;
        }

        /// <summary>
        /// Copies the contents of the list to an array.
        /// </summary>
        /// <returns>An array containing the list's elements.</returns>
        public T[] ToArray()
        {
            var array = new T[Count];
            CopyTo(array, 0);
            return array;
        }

        /// <summary>
        /// Inserts a value at a specific index in the list.
        /// </summary>
        /// <param name="index">The index to insert the value at.</param>
        /// <param name="value">The value to insert.</param>
        public void Insert(int index, T value)
        {
            if (index >= size + 1)
            {
                throw new IndexOutOfRangeException();
            }
            
            EnsureCapacity(size + 1);
            MoveSublist(index, index + 1, Count - index);
            contents[index] = value;
            size++;
        }

        /// <summary>
        /// Removes the first occurrence of an element from the list.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>
        /// <c>true</c> an element was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(T value)
        {
            int index = IndexOf(value);
            if (index < 0)
            {
                return false;
            }
            else
            {
                RemoveAt(index);
                return true;
            }
        }

        /// <summary>
        /// Removes the element at a particular index in the list.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            EnsureInBounds(index);

            MoveSublist(index + 1, index, Count - index - 1);
            size--;
        }

        private void MoveSublist(int oldIndex, int newIndex, int newLength)
        {
            Array.Copy<T>(contents, oldIndex, contents, newIndex, newLength);
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
            CopyTo(newContents, 0);
            contents = newContents;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(contents, 0, size);
        }

        private void EnsureInBounds(int index)
        {
            if (index < 0 || index >= size)
            {
                throw new IndexOutOfRangeException();
            }
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