using System.Primitives;

public sealed class Utf8String
{
    public Utf8String(byte* data)
    {
        this.Length = (int)(uint)strlen(data);
        this.data = data;
    }

    public int Length { get; private set; }

    private byte* data;

    public byte this[int i] => data[i];

    private static extern size_t strlen(byte* str);
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