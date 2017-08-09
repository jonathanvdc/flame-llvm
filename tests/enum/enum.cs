using System;

public enum ComparisonResult
{
    Equal,
    LeftGreater,
    RightGreater
}

public interface IEnumComparable<T>
{
    ComparisonResult Compare(T other);
}

public struct EnumComparableInt : IEnumComparable<EnumComparableInt>
{
    public EnumComparableInt(int value)
    {
        this.value = value;
    }

    public int value;

    public ComparisonResult Compare(EnumComparableInt other)
    {
        if (value == other.value)
            return ComparisonResult.Equal;
        else if (value < other.value)
            return ComparisonResult.RightGreater;
        else
            return ComparisonResult.LeftGreater;
    }
}

public static class Program
{
    public static void Main()
    {
        var a = new EnumComparableInt(10);
        var b = new EnumComparableInt(20);
        Console.WriteLine(a.Compare(b) == ComparisonResult.RightGreater);
    }
}
