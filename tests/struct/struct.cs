using System.Primitives;

public struct Utf8String
{
    public Utf8String(byte* data)
    {
        this.Data = data;
        this.Length = (int)(uint)strlen(data);
    }

    public int Length;
    public byte* Data;

    private extern static size_t strlen(byte* str);
}

public static class Program
{
    public extern static void putchar(byte c);

    private static void WriteLine(Utf8String str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            putchar(str.Data[i]);
        }
        putchar((byte)'\n');
    }

    private static Utf8String ToUtf8String(byte* str)
    {
        return new Utf8String(str);
    }

    public static int Main(int argc, byte* * argv)
    {
        WriteLine(ToUtf8String(argv[1]));
        return 0;
    }
}