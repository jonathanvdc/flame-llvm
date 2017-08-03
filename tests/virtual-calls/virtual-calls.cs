// Test based on an example taken from `virtual (C# Reference)` at MSDN:
// https://msdn.microsoft.com/en-us/library/9fkccyh4(VS.80).aspx

using System;

public class Dimensions
{
    public const double PI = Math.PI;
    protected double x, y;
    public Dimensions()
    {
    }
    public Dimensions(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public virtual double Area()
    {
        return x * y;
    }
}

public class Circle : Dimensions
{
    public Circle(double r) : base(r, 0)
    {
    }

    public override double Area()
    {
        return PI * x * x;
    }
}

class Sphere : Dimensions
{
    public Sphere(double r) : base(r, 0)
    {
    }

    public override double Area()
    {
        return 4 * PI * x * x;
    }
}

class Cylinder : Dimensions
{
    public Cylinder(double r, double h) : base(r, h)
    {
    }

    public override double Area()
    {
        return 2 * PI * x * x + 2 * PI * x * y;
    }
}

class TestClass
{
    static void Main()
    {
        double r = 3.0, h = 5.0;
        Dimensions c = new Circle(r);
        Dimensions s = new Sphere(r);
        Dimensions l = new Cylinder(r, h);
        // Display results:
        Console.Write(((int)c.Area()).ToString() + " ");
        Console.Write(((int)s.Area()).ToString() + " ");
        Console.Write(((int)l.Area()).ToString());
    }
}