using System;

namespace Flame.LLVM
{
    /// <summary>
    /// Describes an unqualified name that has already been mangled.
    /// Name manglers should not try to re-mangle it.
    /// </summary>
    public sealed class PreMangledName : UnqualifiedName
    {
        /// <summary>
        /// Creates a pre-mangled name from the given names.
        /// </summary>
        /// <param name="MangledName">The mangled name.</param>
        /// <param name="UnmangledName">The unmangled name.</param>
        public PreMangledName(string MangledName, UnqualifiedName UnmangledName)
        {
            this.MangledName = MangledName;
            this.UnmangledName = UnmangledName;
        }

        /// <summary>
        /// Gets the name string wrapped by this pre-mangled name.
        /// </summary>
        /// <returns>The name string.</returns>
        public string MangledName { get; private set; }

        /// <summary>
        /// Gets the unmangled version of this pre-mangled name.
        /// </summary>
        /// <returns>The unmangled name.</returns>
        public UnqualifiedName UnmangledName { get; private set; }

        /// <inheritdoc/>
        public override bool Equals(UnqualifiedName Other)
        {
            return Other is PreMangledName && ((PreMangledName)Other).MangledName.Equals(MangledName);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return MangledName.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return MangledName;
        }

        /// <summary>
        /// Gets the unmangled version of the given unqualified name.
        /// </summary>
        /// <param name="Name">The name to examine.</param>
        /// <returns>An unmangled string.</returns>
        public static UnqualifiedName Unmangle(UnqualifiedName Name)
        {
            if (Name is PreMangledName)
            {
                return ((PreMangledName)Name).UnmangledName;
            }
            else
            {
                return Name;
            }
        }
    }
}