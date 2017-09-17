using System;
using IntMap = #builtin_delegate_type(int, int);

public static class Program
{
    private static int Apply(IntMap f, int x)
    {
        return f(x);
    }

    private static int Square(int x)
    {
        return x * x;
    }

    public static void Main()
    {
        Console.WriteLine(Apply(Square, 10));
    }
}