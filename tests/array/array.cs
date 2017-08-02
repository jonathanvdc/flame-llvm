public unsafe static class Program
{
    private static extern ulong strlen(byte* str);
    private static extern void putchar(byte c);

    private static byte[] ToByteArray(byte* buffer, int length)
    {
        var results = new byte[length];
        for (int i = 0; i < length; i++)
        {
            results[i] = buffer[i];
        }
        return results;
    }

    private static byte[] StringToByteArray(byte* buffer)
    {
        return ToByteArray(buffer, (int)strlen(buffer));
    }

    private static void WriteByteArray(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            putchar(data[i]);
        }
        putchar((byte)'\n');
    }

    public static void Main(int argc, byte* * argv)
    {
        for (int i = 1; i < argc; i++)
        {
            byte[] arr = StringToByteArray(argv[i]);
            WriteByteArray(arr);
        }
    }
}