namespace System.Collections.Generic
{
    /// <summary>
    /// A read-only view of a collection of elements.
    /// </summary>
    public interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        int Count { get; }
    }

    /// <summary>
    /// A mutable view of a collection of elements.
    /// </summary>
    public interface ICollection<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Tells if the collection is read-only.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the collection is read-only; otherwise, <c>false</c>.
        /// </returns>
        bool IsReadOnly { get; }

        /// <summary>
        /// Adds an element to the collection.
        /// </summary>
        /// <param name="value">The value to add.</param>
        void Add(T value);

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if the collection contains a value.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>
        /// <c>true</c> if the collection contains the value; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(T value);

        /// <summary>
        /// Copies the contents of the collection to an array, starting
        /// at an offset in the array.
        /// </summary>
        /// <param name="destination">The destination array.</param>
        /// <param name="offset">
        /// The offset where the first element of this collection is placed.
        /// </param>
        void CopyTo(T[] destination, int offset);

        /// <summary>
        /// Removes the first occurrence of an element from the collection.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>
        /// <c>true</c> an element was removed; otherwise, <c>false</c>.
        /// </returns>
        bool Remove(T value);
    }
}