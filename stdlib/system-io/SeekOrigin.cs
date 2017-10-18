namespace System.IO
{
    /// <summary>
    /// Describes an origin from which one can seek.
    /// </summary>
    public enum SeekOrigin
    {
        // NOTE: Begin = 0, Current = 1, End = 2 map to
        // SEEK_SET = 0, SEEK_CUR = 1, SEEK_END = 2 in the
        // C standard library. Changing these constants may
        // break file seeking.

        Begin = 0,
        Current = 1,
        End = 2
    }
}