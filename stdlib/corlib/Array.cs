namespace System
{
    // TODO: tweak the compiler to make all arrays implement 'System.Array'.
    public abstract class Array : Object
    {
        public static void Clear(byte[] array, int index, int length)
        {
            for (int i = 0; i < length; i++)
            {
                array[index + i] = 0;
            }
        }
    }
}