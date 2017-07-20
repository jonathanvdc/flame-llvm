public static unsafe class Program
{
    public static extern int puts(byte* str);

    public static int Main(int argc, byte* * argv)
    {
        for (int i = 1; i < argc; i++)
        {
            puts(argv[i]);
        }
        return 0;
    }
}