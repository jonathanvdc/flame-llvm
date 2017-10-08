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
        public virtual string Message { get; private set; }
    }

    unroll ((TYPE, BASE_TYPE, DEFAULT_MESSAGE) in (
        (SystemException, Exception, "System error."),
        (InvalidOperationException, SystemException, "Operation is not valid due to the current state of the object."),
        (NotSupportedException, SystemException, "Specified method is not supported.")))
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

    /// <summary>
    /// A type of exception that is thrown when at least one argument does
    /// not meet a method's contract.
    /// </summary>
    public class ArgumentException : SystemException
    {
        public ArgumentException()
            : base("Value does not fall within the expected range.")
        { }

        public ArgumentException(string message)
            : base(message)
        { }

        public ArgumentException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public ArgumentException(string message, string paramName, Exception innerException)
            : base(message, innerException)
        {
            this.paramName = paramName;
        }

        public ArgumentException(string message, string paramName)
            : base(message)
        {
            this.paramName = paramName;
        }

        private string paramName;

        /// <inheritdoc/>
        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(paramName))
                {
                    return base.Message + Environment.NewLine +
                        "Parameter name: " + paramName;
                }
                else
                {
                    return base.Message;
                }
            }
        }

        /// <summary>
        /// Gets the name of the parameter that broke a method's contract.
        /// </summary>
        /// <returns>The name of a parameter.</returns>
        public virtual string ParamName
        {
            get { return paramName; }
        }
    }

    unroll ((TYPE, DEFAULT_MESSAGE) in (
        (ArgumentNullException, "Value cannot be null."),
        (ArgumentOutOfRangeException, "Specified argument was out of the range of valid values.")))
    {
        public class TYPE : ArgumentException
        {
            public TYPE()
                : base(DEFAULT_MESSAGE)
            { }

            public TYPE(string paramName)
                : base(DEFAULT_MESSAGE, paramName)
            { }

            public TYPE(string message, Exception innerException)
                : base(message, innerException)
            { }

            public TYPE(string paramName, string message)
                : base(message, paramName)
            { }
        }
    }
}