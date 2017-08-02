using System;

public static class Program
{
    private static double fac(int n)
    {
        if (n <= 1)
            return 1;
        else
            return fac(n - 1) * n;
    }

    public static void Main()
    {
        double num = -fac(10);
        Console.WriteLine((int)num);
    }
}