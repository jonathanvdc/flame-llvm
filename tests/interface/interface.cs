// Test based on an example taken from `interface (C# Reference)` at MSDN:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface

using System;

interface IPoint
{
   // Property signatures:
   int x
   {
      get;
      set;
   }

   int y
   {
      get;
      set;
   }
}

class Point : IPoint
{
   // Fields:
   private int _x;
   private int _y;

   // Constructor:
   public Point(int x, int y)
   {
      _x = x;
      _y = y;
   }

   // Property implementation:
   public int x
   {
      get
      {
         return _x;
      }

      set
      {
         _x = value;
      }
   }

   public int y
   {
      get
      {
         return _y;
      }
      set
      {
         _y = value;
      }
   }
}

class MainClass
{
   static void PrintPoint(IPoint p)
   {
      Console.WriteLine(p.x + " " + p.y);
   }

   static void Main()
   {
      Point p = new Point(2, 3);
      PrintPoint(p);
   }
}