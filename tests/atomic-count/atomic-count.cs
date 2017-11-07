using System;
using System.Collections.Generic;
using System.Threading;

public static class Program
{
    private static int result;
    private const int ManyTimes = 250;

    private static void IncrementManyTimes()
    {
        for (int i = 0; i < ManyTimes; i++)
        {
            Interlocked.Increment(ref result);
        }
    }

    public static void Main()
    {
        // Uses atomics to safely count to 1000. The plan
        // is to start four threads, each of which will
        // atomically increment the result 250 times.
        result = 0;
        var threads = new Thread[4];
        for (int i = 0; i < 4; i++)
        {
            threads[i] = new Thread(IncrementManyTimes);
            threads[i].Start();
        }

        // Wait for the threads to finish.
        for (int i = 0; i < 4; i++) 
        {
            threads[i].Join();
        }

        // Print the result;
        Console.WriteLine(result);
    }
}