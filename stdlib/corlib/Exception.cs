namespace System
{
    /// <summary>
    /// Represents errors that occur during program execution.
    /// </summary>
    public class Exception : Object
    {
        /// <summary>
        /// Creates an exception.
        /// </summary>
        public Exception()
            : this("An exception occurred.")
        { }

        /// <summary>
        /// Creates an exception from an error message.
        /// </summary>
        /// <param name="message">An error message.</param>
        public Exception(string message)
            : this(message, null)
        { }

        /// <summary>
        /// Creates an exception from an error message and an inner
        /// exception.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="innerException">An exception that gives rise to this exception.</param>
        public Exception(string message, Exception innerException)
        {
            this.Message = message;
            this.InnerException = innerException;
        }

        /// <summary>
        /// Gets the inner exception that gave rise to this exception.
        /// </summary>
        /// <returns>The inner exception.</returns>
        public Exception InnerException { get; private set; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>A message that describes why the exception occurred.</returns>
        public string Message { get; private set; }
    }
}