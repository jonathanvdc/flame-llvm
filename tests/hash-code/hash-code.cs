using System;

public static class Program
{
    public static void Main()
    {
        var obj1 = new Object();
        var obj2 = new Object();
        Console.Write(obj1.GetHashCode() == obj2.GetHashCode());
        Console.Write(' ');
        Console.Write(obj1.GetHashCode() == obj1.GetHashCode());
        Console.Write(' ');
        var str1 = "hello world";
        var str2 = "bye";
        var str3 = "hello world";
        Console.Write(str1.GetHashCode() == str2.GetHashCode());
        Console.Write(' ');
        Console.Write(str1.GetHashCode() == str3.GetHashCode());
        Console.WriteLine();
    }
}