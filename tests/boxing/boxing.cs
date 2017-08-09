using System;

public static class Program
{
    public static void Main()
    {
        Object i = 1;
        Object j = 20;
        Object h = 1;
        Object b = true;
        Console.WriteLine(i.Equals(j) + " " + i.Equals(h) + " " + i.Equals(b) + " " + (int)i);
    }
}