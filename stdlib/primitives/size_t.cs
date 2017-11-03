namespace System.Primitives
{
    /// <summary>
    /// A pointer-sized integer value.
    /// </summary>
    public unsafe struct size_t
    {
        /// <summary>
        /// Creates an pointer-sized integer from an integer.
        /// </summary>
        /// <param name="value">A 32-bit unsigned integer.</param>
        public size_t(uint value)
        {
            this = default(size_t);
            this.ptr = (void*)value;
        }

        /// <summary>
        /// Creates an pointer-sized integer from an integer.
        /// </summary>
        /// <param name="value">A 64-bit unsigned integer.</param>
        public size_t(ulong value)
        {
            this = default(size_t);
            this.ptr = (void*)value;
        }

        /// <summary>
        /// Creates an pointer-sized integer from a pointer.
        /// </summary>
        /// <param name="value">An opaque pointer.</param>
        public size_t(void* value)
        {
            this = default(size_t);
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
        public static size_t Zero => new size_t((void*)null);

        /// <summary>
        /// Converts this value to a 32-bit unsigned integer.
        /// </summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        public uint ToUInt32()
        {
            return (uint)ptr;
        }

        /// <summary>
        /// Converts this value to a 64-bit unsigned integer.
        /// </summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        public ulong ToUInt64()
        {
            return (ulong)ptr;
        }

        /// <summary>
        /// Converts this value to an opaque pointer.
        /// </summary>
        /// <returns>An opaque pointer.</returns>
        public void* ToPointer()
        {
            return ptr;
        }

        /// <summary>
        /// Adds an offset to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to add, in bytes.</param>
        /// <returns>The base pointer plus the offset.</returns>
        public static size_t Add(size_t pointer, int offset)
        {
            return new size_t((byte*)pointer.ptr + offset);
        }

        /// <summary>
        /// Subtracts an offset from a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to subtract, in bytes.</param>
        /// <returns>The base pointer minus the offset.</returns>
        public static size_t Subtract(size_t pointer, int offset)
        {
            return Add(pointer, -offset);
        }

        /// <summary>
        /// Determines if two pointer-sized integers are equal.
        /// </summary>
        /// <param name="value1">The first pointer-sized integer.</param>
        /// <param name="value2">The second pointer-sized integer.</param>
        /// <returns><c>true</c> if the pointer-sized integers are equal; otherwise, <c>false</c>.</returns>
        public static bool operator==(size_t value1, size_t value2)
        {
            return value1.ptr == value2.ptr;
        }

        /// <summary>
        /// Determines if two pointer-sized integers are not equal.
        /// </summary>
        /// <param name="value1">The first pointer-sized integer.</param>
        /// <param name="value2">The second pointer-sized integer.</param>
        /// <returns><c>true</c> if the pointer-sized integers are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator!=(size_t value1, size_t value2)
        {
            return value1.ptr != value2.ptr;
        }

        /// <summary>
        /// Adds an offset to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to add, in bytes.</param>
        /// <returns>The base pointer plus the offset.</returns>
        public static size_t operator+(size_t pointer, int offset)
        {
            return Add(pointer, offset);
        }

        /// <summary>
        /// Subtracts an offset from a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">The base pointer, as a pointer-sized integer.</param>
        /// <param name="offset">The offset to subtract, in bytes.</param>
        /// <returns>The base pointer minus the offset.</returns>
        public static size_t operator-(size_t pointer, int offset)
        {
            return Subtract(pointer, offset);
        }

        /// <summary>
        /// Converts a pointer-sized integer to a 32-bit unsigned integer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>A 32-bit unsigned integer.</returns>
        public static explicit operator uint(size_t pointer)
        {
            return pointer.ToUInt32();
        }

        /// <summary>
        /// Converts a pointer-sized integer to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>A 64-bit unsigned integer.</returns>
        public static explicit operator ulong(size_t pointer)
        {
            return pointer.ToUInt64();
        }

        /// <summary>
        /// Converts a pointer-sized integer to an opaque pointer.
        /// </summary>
        /// <param name="pointer">A pointer-sized integer.</param>
        /// <returns>An opaque pointer.</returns>
        public static explicit operator void*(size_t pointer)
        {
            return pointer.ToPointer();
        }

        /// <summary>
        /// Converts a 32-bit unsigned integer to a pointer-sized integer.
        /// </summary>
        /// <param name="value">A 32-bit unsigned integer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator size_t(uint value)
        {
            return new size_t(value);
        }

        /// <summary>
        /// Converts a 64-bit unsigned integer to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">A 64-bit unsigned integer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator size_t(ulong value)
        {
            return new size_t(value);
        }

        /// <summary>
        /// Converts an opaque pointer to a pointer-sized integer.
        /// </summary>
        /// <param name="pointer">An opaque pointer.</param>
        /// <returns>A pointer-sized integer.</returns>
        public static explicit operator size_t(void* pointer)
        {
            return new size_t(pointer);
        }
    }
}