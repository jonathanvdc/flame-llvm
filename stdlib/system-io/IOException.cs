namespace System.IO
{
    public class IOException : SystemException
    {
        public IOException()
            : base("An IO exception occurred.")
        { }

        public IOException(string message)
            : base(message)
        { }

        public IOException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}