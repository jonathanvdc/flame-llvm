public unsafe struct Utf8String
{
    public Utf8String(byte* data)
    {
        this.Length = (int)strlen(data);
        this.Data = data;
    }

    /// <summary>
    /// Gets this UTF-8 string's length.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets this UTF-8 string's data, as an array of bytes.
    /// </summary>
    public byte* Data { get; private set; }

    /// <summary>
    /// Gets the ith byte in this UTF-8 string.
    /// </summary>
    public byte this[int i] => Data[i];

    private static extern ulong strlen(byte* data);
}

public unsafe static class Console
{
    private static extern void putchar(byte c);

    public static void Write(Utf8String str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            putchar(str[i]);
        }
    }

    public static void WriteLine()
    {
        putchar((byte)'\n');
    }

    public static void WriteLine(Utf8String str)
    {
        Write(str);
        WriteLine();
    }
}

public unsafe static class Program
{
    public static void Main(int argc, byte* * argv)
    {
        for (int i = 1; i < argc; i++)
        {
            Console.WriteLine(new Utf8String(argv[i]));
        }
    }
}