namespace System
{
    /// <summary>
    /// Defines common mathematical functions and operations.
    /// </summary>
    public static class Math
    {
        public const double PI = 3.14159265358979;

        /// <summary>
        /// Gets the largest of the given two integers.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns><c>a</c> or <c>b</c>, whichever is larger.</returns>
        public static int Max(int a, int b)
        {
            return a > b ? a : b;
        }

        /// <summary>
        /// Gets the smallest of the given two integers.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns><c>a</c> or <c>b</c>, whichever is smaller.</returns>
        public static int Min(int a, int b)
        {
            return a < b ? a : b;
        }
    }
}