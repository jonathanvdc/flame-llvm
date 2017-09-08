// This file makes use of the LeMP 'unroll' macro to avoid copy-pasting code.
// See http://ecsharp.net/lemp/avoid-tedium-with-LeMP.html for an explanation.

#importMacros(LeMP);

namespace System
{
    /// <summary>
    /// Defines common mathematical functions and operations.
    /// </summary>
    public static class Math
    {
        public const double PI = 3.14159265358979;

        unroll ((TYPE) in (byte, sbyte, ushort, short, uint, int, ulong, long, float, double))
        {
            /// <summary>
            /// Gets the largest of the given two values.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns><c>a</c> or <c>b</c>, whichever is larger.</returns>
            public static TYPE Max(TYPE a, TYPE b)
            {
                return a > b ? a : b;
            }

            /// <summary>
            /// Gets the smallest of the given two values.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns><c>a</c> or <c>b</c>, whichever is smaller.</returns>
            public static TYPE Min(TYPE a, TYPE b)
            {
                return a < b ? a : b;
            }
        }
    }
}