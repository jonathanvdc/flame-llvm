using System;
using System.Threading;
using static System.Console;

public static class Program
{
    public static void Main()
    {
        int dest = 10;
        int value = 5;
        int comparand = 10;
        int oldValue = Interlocked.CompareExchange(ref dest, value, comparand);
        int oldComparand = Interlocked.Exchange(ref comparand, dest);
        WriteLine(oldValue + " " + oldComparand + " " + comparand);
    }
}