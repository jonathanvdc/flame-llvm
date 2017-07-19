public static unsafe class Program
{
    public static extern void* malloc(ulong size);
    public static extern void free(void* data);
    public static extern int puts(byte* str);

    public static int main()
    {
        byte* str = (byte*)malloc(3);
        *str = (byte)'h';
        *(str + 1) = (byte)'i';
        *(str + 2) = (byte)'\0';
        puts(str);
        free((void*)str);
        return 0;
    }
}