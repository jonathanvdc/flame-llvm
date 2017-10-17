namespace System.IO
{
    /// <summary>
    /// Specifies how a file should be opened.
    /// </summary>
    public enum FileMode
    {
        /// <summary>
        /// Opens the file if it exists and seeks to the end of the file, or creates a new file.
        /// Can be used only if a file is opened in write-only mode. Seeking to a position before
        /// the end of the file is not supported.
        /// </summary>
        Append,

        /// <summary>
        /// Requests the creation of a file. If the file already exists, then it is first
        /// cleared and then overwritten.
        /// </summary>
        Create,

        /// <summary>
        /// Requests the creation of a new file. If the file already exists, an exception is thrown.
        /// </summary>
        CreateNew,

        /// <summary>
        /// Specifies that a file should be opened. An exception is thrown if the file already
        /// exists.
        /// </summary>
        Open,

        /// <summary>
        /// Specifies that a file should be opened if it exists already and created if it does not.
        /// </summary>
        OpenOrCreate,

        /// <summary>
        /// Opens an existing file, clears it contents and overwrites it.
        /// </summary>
        Truncate
    }
}