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
        /// Creates a pre-mangled name from the given name string.
        /// </summary>
        /// <param name="Name">The name string.</param>
        public PreMangledName(string Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// Gets the name string wrapped by this pre-mangled name.
        /// </summary>
        /// <returns>The name string.</returns>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override bool Equals(UnqualifiedName Other)
        {
            return Other is PreMangledName && ((PreMangledName)Other).Name.Equals(Name);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}