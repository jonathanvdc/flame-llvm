public static unsafe class Program
{
    public static extern void* malloc(ulong size);
    public static extern void free(void* data);
    public static extern int puts(byte* str);

    public static void FillString(byte* str)
    {
        *str = (byte)'h';
        *(str + 1) = (byte)'i';
        *(str + 2) = (byte)'\0';
    }

    public static int Main()
    {
        byte* str = (byte*)malloc(3);
        FillString(str);
        puts(str);
        free((void*)str);
        str = null;
        return 0;
    }
}