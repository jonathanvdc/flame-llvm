namespace System
{
    /// <summary>
    /// Defines common functionality for values that manage resources
    /// which can be disposed.
    /// </summary>
    public interface IDisposable
    {
        /// <summary>
        /// Disposes the resources managed by this value.
        /// </summary>
        void Dispose();
    }
}