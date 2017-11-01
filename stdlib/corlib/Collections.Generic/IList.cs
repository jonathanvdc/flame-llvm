namespace System.Collections.Generic
{
    /// <summary>
    /// A read-only view of a list data structure.
    /// </summary>
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Gets the element at a particular index in the list.
        /// </summary>
        T this[int i] { get; }
    }

    /// <summary>
    /// A mutable view of a list data structure.
    /// </summary>
    public interface IList<T> : ICollection<T>
    {
        /// <summary>
        /// Gets or sets the element at a particular index in the list.
        /// </summary>
        T this[int i] { get; set; }

        /// <summary>
        /// Gets the index in the list of the first occurrence of a value.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>
        /// The index of the first occurrence of a value, if it is in the list;
        /// otherwise, a negative value.
        /// </returns>
        int IndexOf(T value);

        /// <summary>
        /// Inserts a value at a specific index in the list.
        /// </summary>
        /// <param name="index">The index to insert the value at.</param>
        /// <param name="value">The value to insert.</param>
        void Insert(int index, T value);

        /// <summary>
        /// Removes the element at a particular index in the list.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        void RemoveAt(int index);
    }
}