using System;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.Write(args[0]);
            for (int i = 1; i < args.Length; i++)
            {
                Console.Write(' ');
                Console.Write(args[i]);
            }
        }
        Console.WriteLine();
    }
}