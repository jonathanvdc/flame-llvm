using System;
using System.Threading;

public class SumTask
{
    public SumTask(int ThreadId, int[] Inputs, int[] Outputs)
    {
        this.ThreadId = ThreadId;
        this.Inputs = Inputs;
        this.Outputs = Outputs;
    }

    public int ThreadId { get; private set; }
    public int[] Inputs { get; private set; }
    public int[] Outputs { get; private set; }

    public void Run()
    {
        int sum = 0;
        foreach (var i in Inputs)
        {
            sum += i;
        }

        Outputs[ThreadId] = sum;
    }
}

public static class Program
{
    private static int[] values = new int[] { 1, 2, 3, 4, 5, 6 };

    private static bool CheckAllEqual(int[] items)
    {
        if (items.Length <= 1)
            return true;

        int first = items[0];
        for (int i = 1; i < items.Length; i++)
        {
            if (items[i] != first)
                return false;
        }

        return true;
    }

    public static void Main()
    {
        var results = new int[4];

        var threads = new Thread[results.Length];
        for (int i = 0; i < results.Length; i++)
        {
            threads[i] = new Thread(new SumTask(i, values, results).Run);
            threads[i].Start();
        }

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Join();
        }

        Console.WriteLine(
            CheckAllEqual(results)
            ? "Computation successful. Result: " + results[0] + "."
            : "Diverging results. Something went wrong.");
    }
}