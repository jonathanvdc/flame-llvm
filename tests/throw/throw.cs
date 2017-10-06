using System;

public static class Program
{
    public static void Main()
    {
        try
        {
            ThrowAndFinally();
        }
        catch (Exception o)
        {
            Console.Write("are ");
        }
        finally
        {
            Console.WriteLine("ya");
        }
    }

    private static void ThrowAndFinally()
    {
        try
        {
            Throw();
        }
        finally
        {
            Console.Write("how ");
        }
    }

    private static void Throw()
    {
        throw new InvalidOperationException();
    }
}