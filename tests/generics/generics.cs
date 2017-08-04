using System;

public class Box<T>
{
    public Box(T value)
    {
        this.value = value;
    }

    public T value;
}

public static class Program
{
    public static void Main()
    {
        var box = new Box<int>(14);
        Console.Write(box.value);
        Console.Write(' ');
        box.value = 42;
        Console.Write(box.value);
        Console.WriteLine();
    }
}