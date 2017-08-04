using System;

public interface IConsoleWriteable
{
    void WriteToConsole();
}

public class ConsoleWriteableInt : IConsoleWriteable
{
    public ConsoleWriteableInt(int value)
    {
        this.value = value;
    }
    
    private int value;

    public void WriteToConsole()
    {
        Console.Write(value);
    }
}

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
    private static void PrintRefBox<T>(Box<T> refBox)
        where T : class, IConsoleWriteable
    {
        refBox.value.WriteToConsole();
    }

    public static void Main()
    {
        var valBox = new Box<int>(14);
        Console.Write(valBox.value);
        Console.Write(' ');
        var refBox = new Box<ConsoleWriteableInt>(null);
        refBox.value = new ConsoleWriteableInt(42);
        PrintRefBox<ConsoleWriteableInt>(refBox);
        Console.WriteLine();
    }
}