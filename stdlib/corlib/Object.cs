namespace System
{
    /// <summary>
    /// The root type in the type system.
    /// </summary>
    [#builtin_attribute(RootTypeAttribute)]
    public class Object
    {
        /// <summary>
        /// Creates an object.
        /// </summary>
        [#builtin_attribute(NopAttribute)]
        public Object()
        {

        }

        /// <summary>
        /// Releases resources related to this object just before its storage
        /// is recycled by the garbage collector.
        /// </summary>
        [#builtin_attribute(NopAttribute)]
        protected virtual void Finalize()
        {

        }

        /// <summary>
        /// Checks if this object is equal to the given value.
        /// </summary>
        /// <param name="other">The value to compare this object to.</param>
        /// <returns><c>true</c> if this object is equal to <c>other</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Equals(Object other)
        {
            return this == other;
        }

        /// <summary>
        /// Computes a hash code for this object.
        /// </summary>
        /// <returns>A hash code.</returns>
        public virtual int GetHashCode()
        {
            return (int)#builtin_ref_to_ptr(this);
        }

        /// <summary>
        /// Gets a string representation for this object.
        /// </summary>
        /// <returns>A string representation.</returns>
        public virtual string ToString()
        {
            return "<object at " + Convert.ToString((long)#builtin_ref_to_ptr(this)) + ">";
        }

        /// <summary>
        /// Checks if the given objects are the same, i.e., they both
        /// point to the same storage.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>
        /// <c>true</c> if both arguments point to the same storage; otherwise, <c>false</c>.
        /// </returns>
        public static bool ReferenceEquals(Object first, Object second)
        {
            return first == second;
        }

        /// <summary>
        /// Checks if the given objects are equal.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>
        /// <c>true</c> if both arguments point to the same storage or if
        /// <c>first.Equals(second)</c> evaluates to <c>true</c>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(Object first, Object second)
        {
            if (first == second)
            {
                return true;
            }
            else if (first == null || second == null)
            {
                return false;
            }
            else
            {
                return first.Equals(second);
            }
        }
    }
}