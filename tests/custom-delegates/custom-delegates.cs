using System;

public delegate int IntMap(int x);
public delegate T2 Map<T1, T2>(T1 x);

public static class Program
{
    private static int ApplyIntMap(IntMap f, int x)
    {
        return f(x);
    }

    private static int ApplyMap(Map<int, int> f, int x)
    {
        return f(x);
    }

    private static int Square(int x)
    {
        return x * x;
    }

    public static void Main()
    {
        Console.Write(ApplyIntMap(Square, 10));
        Console.Write(' ');
        Console.WriteLine(ApplyMap(Square, 10));
    }
}