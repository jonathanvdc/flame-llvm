// Test based on an example taken from `abstract (C# Reference)` at MSDN:
// https://msdn.microsoft.com/en-us/library/sf985hc5(VS.71).aspx

using System;
abstract class MyBaseC   // Abstract class
{
   protected int x = 100;
   protected int y = 150;
   public abstract void MyMethod();   // Abstract method

   public abstract int GetX   // Abstract property
   {
      get;
   }

   public abstract int GetY   // Abstract property
   {
      get;
   }
}

class MyDerivedC: MyBaseC
{
   public override void MyMethod()
   {
      x++;
      y++;
   }

   public override int GetX   // overriding property
   {
      get
      {
         return x+10;
      }
   }

   public override int GetY   // overriding property
   {
      get
      {
         return y+10;
      }
   }

   public static void Main()
   {
      MyDerivedC mC = new MyDerivedC();
      mC.MyMethod();
      Console.WriteLine(mC.GetX.ToString() + " " + mC.GetY.ToString());
   }
}