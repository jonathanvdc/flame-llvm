namespace System
{
    /// <summary>
    /// The root type in the type system.
    /// </summary>
    public class Object
    {
        public Object()
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

        public static bool ReferenceEquals(Object first, Object second)
        {
            return first == second;
        }

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