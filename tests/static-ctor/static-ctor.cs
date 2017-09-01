using System;

public sealed class Singleton
{
    static Singleton()
    {
        Instance = new Singleton();
        Instance.message = "hello world";
    }

    private Singleton() { }

    public static readonly Singleton Instance;

    private string message;

    public void SayHi()
    {
        Console.WriteLine(message);
    }
}

public static class Program
{
    public static void Main()
    {
        Singleton.Instance.SayHi();
    }
}