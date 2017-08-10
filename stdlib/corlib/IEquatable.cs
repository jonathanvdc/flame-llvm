namespace System
{
    /// <summary>
    /// An interface for types that can be compared to instances of a specific type.
    /// </summary>
    public interface IEquatable<T>
    {
        /// <summary>
        /// Tests if this value equals the given value.
        /// </summary>
        /// <param name="other">The value to compare this value to.</param>
        /// <returns>
        /// <c>true</c> if this value equals the given value; otherwise, <c>false</c>.
        /// </returns>
        bool Equals(T other);
    }
}