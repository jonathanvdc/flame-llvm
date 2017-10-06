namespace System
{
    /// <summary>
    /// Represents a value type that can be assigned null.
    /// </summary>
    public struct Nullable<T> : Object
        where T : struct
    {
        /// <summary>
        /// Creates a non-null nullable object from the given value.
        /// </summary>
        /// <param name="value">The inner value.</param>
        public Nullable(T value)
        {
            this.value = value;
            this.HasValue = true;
        }

        // The nullable's value.
        private T value;

        /// <summary>
        /// Records if this nullable has a value, i.e., is non-null.
        /// </summary>
        /// <returns><c>true</c> if this nullable has a value; otherwise, <c>false</c>.</returns>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Gets the value of this nullable if it is non-null.
        /// </summary>
        /// <returns>This nullable's value.</returns>
        public T Value
        {
            get
            {
                // if (!HasValue)
                // {
                //     throw new InvalidOperationException();
                // }
                return value;
            }
        }

        /// <summary>
        /// Retrieves the value of this nullable or the default value.
        /// </summary>
        /// <returns>
        /// The value of this nullable if it has one; otherwise, the default value.
        /// </returns>
        public T GetValueOrDefault()
        {
            return value;
        }

        /// <summary>
        /// Retrieves the value of this nullable or the given default value.
        /// </summary>
        /// <returns>
        /// The value of this nullable if it has one; otherwise, the given value.
        /// </returns>
        public T GetValueOrDefault(T defaultValue)
        {
            return HasValue ? value : defaultValue;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return !HasValue;
            }
            else if (obj is T)
            {
                return HasValue && value.Equals(obj);
            }
            else if (obj is Nullable<T>)
            {
                var other = (Nullable<T>)obj;
                if (HasValue)
                {
                    return other.HasValue && value.Equals(other.value);
                }
                else
                {
                    return !other.HasValue;
                }
            }
            else
            {
                return false;
            }
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (HasValue)
                return value.GetHashCode();
            else
                return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (HasValue)
                return value.ToString();
            else
                return "";
        }

        /// <summary>
        /// Explicitly converts the given nullable to its underlying value.
        /// </summary>
        /// <param name="value">The nullable to convert.</param>
        /// <returns>The nullable's underlying value.</returns>
        public static explicit operator T(Nullable<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Implicitly converts the given value to a nullable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A nullable that wraps the value.</returns>
        public static implicit operator Nullable<T>(T value)
        {
            return new Nullable<T>(value);
        }
    }
}