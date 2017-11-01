namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a collection of elements.
    /// </summary>
    public interface IEnumerable<out T>
    {
        /// <summary>
        /// Produces an enumerator that iterates through a collection of elements.
        /// </summary>
        /// <returns>An enumerator.</returns>
        IEnumerator<T> GetEnumerator();
    }

    /// <summary>
    /// Represents an iteration over a collection of elements.
    /// </summary>
    public interface IEnumerator<out T> : IDisposable
    {
        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>The current element.</returns>
        T Current { get; }

        /// <summary>
        /// Tries to move to the next element in the collection.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there was a next element in the collection; otherwise, <c>false</c>.
        /// </returns>
        bool MoveNext();

        /// <summary>
        /// Resets the enumerator.
        /// </summary>
        void Reset();
    }
}