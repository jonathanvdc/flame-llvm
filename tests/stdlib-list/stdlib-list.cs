using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main()
    {
        var list = new List<int>();
        list.Add(10);
        list.Add(42);
        list.Insert(0, 90);
        list.Remove(90);
        Console.Write(list[0]);
        for (int i = 1; i < list.Count; i++)
        {
            Console.Write(' ');
            Console.Write(list[i]);
        }
        Console.WriteLine();
    }
}