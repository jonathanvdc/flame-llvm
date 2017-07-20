public static unsafe class Program
{
    public static extern void* malloc(ulong size);
    public static extern void free(void* data);
    public static extern int puts(byte* str);
    public static extern int atoi(byte* str);

    private static byte* itoa(int i, byte* b)
    {
        // Based on the code from bhuwansahni's answer to this StackOverflow
        // question:
        // https://stackoverflow.com/questions/9655202/how-to-convert-integer-to-string-in-c
        byte* p = b;
        if (i < 0)
        {
            *p++ = (byte)'-';
            i = -i;
        }
        int shifter = i;
        do
        {
            // Move to where representation ends
            ++p;
            shifter /= 10;
        } while (shifter != 0);
        *p = (byte)'\0';
        do
        {
            // Move back, inserting digits as u go
            *--p = (byte)(i % 10 + (int)'0');
            i /= 10;
        } while (i != 0);
        return b;
    }

    public static int Fibonacci(int n)
    {
        int ultimate = 1;
        int penultimate = 1;
        for (int i = 2; i < n; i++)
        {
            int newUltimate = ultimate + penultimate;
            penultimate = ultimate;
            ultimate = newUltimate;
        }
        return ultimate;
    }

    public static int Main(int argc, byte* * argv)
    {
        int n = argc == 2 ? atoi(argv[1]) : 5;
        byte* str = (byte*)malloc(15);
        itoa(Fibonacci(n), str);
        puts(str);
        free((void*)str);
        str = null;
        return 0;
    }
}