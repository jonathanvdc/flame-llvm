namespace System.Collections.Generic
{
    /// <summary>
    /// A strongly-typed array list.
    /// </summary>
    public class List<T> : Object
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
                ResizeCapacity(2 * Capacity);
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
    }
}