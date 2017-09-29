using System;

public class Node
{
    ~Node()
    {
        if (next == null)
            Console.WriteLine(value);
    }

    public int value;
    public Node next;
}

public static class Program
{
    public static void Main()
    {
        var node1 = new Node() { value = 1 };
        var node2 = new Node() { value = 3 };
        node1.next = node2;
    }
}