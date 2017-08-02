using System;

public class Point2
{
    public Point2(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }

    public int X;
    public int Y;
}

public class Point3 : Point2
{
    public Point3(int X, int Y, int Z)
        : base(X, Y)
    {
        this.Z = Z;
    }

    public int Z;
}

public class Point4 : Point3
{
    public Point3(int X, int Y, int Z, int W)
        : base(X, Y, Z)
    {
        this.W = W;
    }

    public int W;
}

public static class Program
{
    public static void Main()
    {
        var pt = new Point4(0, -22, 45, 100);
        Console.Write(pt.X);
        Console.Write(' ');
        Console.Write(pt.Y);
        Console.Write(' ');
        Console.Write(pt.Z);
        Console.Write(' ');
        Console.Write(pt.W);
        Console.Write(' ');
        var pt2 = pt as Point2;
        var pt3 = (Point4)pt2;
        Console.Write(pt3 is Point2);
        Console.Write(' ');
        Console.Write(pt3 is Point3);
        Console.Write(' ');
        Console.Write(pt3 is Point4);
        var pt4 = new Point2(0, 0);
        Console.Write(' ');
        Console.Write(pt4 is Point2);
        Console.Write(' ');
        Console.Write(pt4 is Point3);
        Console.Write(' ');
        Console.Write(pt4 is Point4);
        Console.WriteLine();
    }
}