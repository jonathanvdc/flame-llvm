// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

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

    unroll ((TYPE, BASE_TYPE, DEFAULT_MESSAGE) in (
        (SystemException, Exception, "System error."),
        (InvalidOperationException, SystemException, "Operation is not valid due to the current state of the object.")))
    {
        public class TYPE : BASE_TYPE
        {
            /// <summary>
            /// Creates an exception.
            /// </summary>
            public TYPE()
                : base(DEFAULT_MESSAGE)
            { }

            /// <summary>
            /// Creates an exception from an error message.
            /// </summary>
            /// <param name="message">An error message.</param>
            public TYPE(string message)
                : base(message)
            { }

            /// <summary>
            /// Creates an exception from an error message and an inner
            /// exception.
            /// </summary>
            /// <param name="message">An error message.</param>
            /// <param name="innerException">An exception that gives rise to this exception.</param>
            public TYPE(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
    }
}