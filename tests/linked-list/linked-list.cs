using System;

public class Node
{
    public Node(int data)
    {
        this.data = data;
        this.successor = null;
    }

    public int data;
    public Node successor;
}

public static class Program
{
    private static void PrintList(Node node)
    {
        Console.Write(node.data);
        node = node.successor;
        while (node != null)
        {
            Console.Write(' ');
            Console.Write(node.data);
            node = node.successor;
        }
    }

    private static Node CreatePow2List(int n)
    {
        var result = new Node(1);
        var last = result;
        for (int i = 0; i < n; i++)
        {
            last.successor = new Node(last.data * 2);
            last = last.successor;
        }
        return result;
    }

    public static void Main()
    {
        PrintList(CreatePow2List(10));
    }
}