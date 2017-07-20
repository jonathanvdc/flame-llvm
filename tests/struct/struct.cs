public struct Utf8String
{
    public Utf8String(byte* data, int length)
    {
        this.Data = data;
        this.Length = length;
    }

    public int Length;
    public byte* Data;
}

public static class Program
{
    public extern static ulong strlen(byte* str);
    public extern static void putchar(byte c);

    private static void WriteLine(Utf8String str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            putchar(str.Data[i]);
        }
        putchar('\n');
    }

    private static Utf8String ToUtf8String(byte* str)
    {
        return new Utf8String(str, (int)strlen(str));
    }

    public static int Main(int argc, byte* * argv)
    {
        WriteLine(ToUtf8String(argv[1]));
        return 0;
    }
}