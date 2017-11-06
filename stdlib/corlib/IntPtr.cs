namespace System
{
    /// <summary>
    /// A pointer-sized integer value.
    /// </summary>
    public unsafe struct IntPtr
    {
        /// <summary>
        /// Creates an pointer-sized integer from an integer.
        /// </summary>
        /// <param name="value">A 32-bit signed integer.</param>
        public IntPtr(int value)
        {
            this = default(IntPtr);
            this.ptr = (void*)value;
        }

        /// <summary>
        /// Creates an pointer-sized integer from an integer.
        /// </summary>
        /// <param name="value">A 64-bit signed integer.</param>
        public IntPtr(long value)
        {
            this = default(IntPtr);
            this.ptr = (void*)value;
        }

        /// <summary>
        /// Creates an pointer-sized integer from a pointer.
        /// </summary>
        /// <param name="value">An opaque pointer.</param>
        public IntPtr(void* value)
        {
            this = default(IntPtr);
            this.ptr = value;
        }

        private void* ptr;

        /// <summary>
        /// Gets the size of a pointer-sized integer.
        /// </summary>
        /// <returns>The size of a pointer-sized integer.</returns>
        public static int Size => sizeof(void*);

        /// <summary>
        /// A read-only field that represents a pointer-sized integer that has
        /// been initialized to zero.
        /// </summary>
        /// <returns>Zero as a pointer-sized integer.</returns>
        public static IntPtr Zero => new IntPtr(null);

        /// <summary>
        /// Converts this value to a 32-bit signed integer.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        public int ToInt32()
        {
            return (int)ptr;
        }

        /// <summary>
        /// Converts this value to a 64-bit signed integer.
        /// </summary>
        /// <returns>A 64-bit signed integer.</returns>
        public long ToInt64()
        {
            return (long)ptr;
        }

        /// <summary>
        /// Converts this value to an opaque pointer.
        /// </summary>
        /// <returns>An opaque pointer.</returns>
        public void* ToPointer()
        {
            return ptr;
        }

        /// <inheritdoc/>
        public override bool Equals(Object obj)
        {
            return obj is IntPtr && ptr == ((IntPtr)obj).ptr;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToInt64().GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            // TODO: we should probably format this as hex instead.
            return ToInt64().ToString();
        }

        /// <summary>
        /// Adds an offset to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to add, in bytes.</param>
        /// <returns>The base pointer plus the offset.</returns>
        public static IntPtr Add(IntPtr pointer, int offset)
        {
            return new IntPtr((byte*)pointer.ptr + offset);
        }

        /// <summary>
        /// Subtracts an offset from a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to subtract, in bytes.</param>
        /// <returns>The base pointer minus the offset.</returns>
        public static IntPtr Subtract(IntPtr pointer, int offset)
        {
            return Add(pointer, -offset);
        }

        /// <summary>
        /// Determines if two pointer-sized integers are equal.
        /// </summary>
        /// <param name="value1">The first pointer-sized integer.</param>
        /// <param name="value2">The second pointer-sized integer.</param>
        /// <returns><c>true</c> if the pointer-sized integers are equal; otherwise, <c>false</c>.</returns>
        public static bool operator==(IntPtr value1, IntPtr value2)
        {
            return value1.ptr == value2.ptr;
        }

        /// <summary>
        /// Determines if two pointer-sized integers are not equal.
        /// </summary>
        /// <param name="value1">The first pointer-sized integer.</param>
        /// <param name="value2">The second pointer-sized integer.</param>
        /// <returns><c>true</c> if the pointer-sized integers are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator!=(IntPtr value1, IntPtr value2)
        {
            return value1.ptr != value2.ptr;
        }

        /// <summary>
        /// Adds an offset to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to add, in bytes.</param>
        /// <returns>The base pointer plus the offset.</returns>
        public static IntPtr operator+(IntPtr pointer, int offset)
        {
            return Add(pointer, offset);
        }

        /// <summary>
        /// Subtracts an offset from a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to subtract, in bytes.</param>
        /// <returns>The base pointer minus the offset.</returns>
        public static IntPtr operator-(IntPtr pointer, int offset)
        {
            return Subtract(pointer, offset);
        }

        /// <summary>
        /// Converts a pointer-sized integer to a 32-bit signed integer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>A 32-bit signed integer.</returns>
        public static explicit operator int(IntPtr pointer)
        {
            return pointer.ToInt32();
        }

        /// <summary>
        /// Converts a pointer-sized integer to a 64-bit signed integer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>A 64-bit signed integer.</returns>
        public static explicit operator long(IntPtr pointer)
        {
            return pointer.ToInt64();
        }

        /// <summary>
        /// Converts a pointer-sized integer to an opaque pointer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>An opaque pointer.</returns>
        public static explicit operator void*(IntPtr pointer)
        {
            return pointer.ToPointer();
        }

        /// <summary>
        /// Converts a 32-bit signed integer to a pointer-sized integer.
        /// </summary>
        /// <param name="value">A 32-bit signed integer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        /// <summary>
        /// Converts a 64-bit signed integer to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">A 64-bit signed integer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator IntPtr(long value)
        {
            return new IntPtr(value);
        }

        /// <summary>
        /// Converts an opaque pointer to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">An opaque pointer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator IntPtr(void* pointer)
        {
            return new IntPtr(pointer);
        }
    }
}