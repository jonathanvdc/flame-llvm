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

        public virtual bool Equals(Object other)
        {
            return this == other;
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