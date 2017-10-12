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

public unsafe static class Program
{
    private static extern void putchar(byte c);

    private static void WriteLine(Utf8String str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            putchar(str.Data[i]);
        }
        putchar((byte)'\n');
    }

    private static Utf8String firstArg;

    public static void Main(int argc, byte* * argv)
    {
        firstArg = new Utf8String(argv[1]);
        WriteLine(firstArg);
    }
}