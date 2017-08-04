namespace Flame.LLVM
{
    /// <summary>
    /// Describes common functionality implemented by name manglers.
    /// </summary>
    public abstract class NameMangler
    {
        /// <summary>
        /// Gets the given method's mangled name.
        /// </summary>
        /// <param name="Method">The method whose name is to be mangled.</param>
        /// <param name="MangleFullName">
        /// If this is <c>true</c>, then the method's full name and signature is
        /// mangled. Otherwise, only its name is mangled.
        /// </param>
        /// <returns>The mangled name.</returns>
        public abstract string Mangle(IMethod Method, bool MangleFullName);

        /// <summary>
        /// Gets the given field's mangled name.
        /// </summary>
        /// <param name="Field">The field whose name is to be mangled.</param>
        /// <param name="MangleFullName">
        /// If this is <c>true</c>, then the field's full name is
        /// mangled. Otherwise, only its name is mangled.
        /// </param>
        /// <returns>The mangled name.</returns>
        public abstract string Mangle(IField Method, bool MangleFullName);

        /// <summary>
        /// Gets the given type's mangled name.
        /// </summary>
        /// <param name="Type">The type whose name is to be mangled.</param>
        /// <param name="MangleFullName">
        /// If this is <c>true</c>, then the type's full name is
        /// mangled. Otherwise, only its name is mangled.
        /// </param>
        /// <returns>The mangled name.</returns>
        public abstract string Mangle(IType Type, bool MangleFullName);
    }

    /// <summary>
    /// A name mangler implementation for C compatibility.
    /// Names are left neither mangled nor prefixed.
    /// </summary>
    public sealed class CMangler : NameMangler
    {
        private CMangler() { }

        /// <summary>
        /// An instance of a C name mangler.
        /// </summary>
        public static readonly CMangler Instance = new CMangler();

        /// <inheritdoc/>
        public override string Mangle(IMethod Method, bool MangleFullName)
        {
            return Method.Name.ToString();
        }

        /// <inheritdoc/>
        public override string Mangle(IField Field, bool MangleFullName)
        {
            return Field.Name.ToString();
        }

        /// <inheritdoc/>
        public override string Mangle(IType Type, bool MangleFullName)
        {
            if (MangleFullName)
            {
                return Type.FullName.ToString();
            }
            else
            {
                return Type.Name.ToString();
            }
        }
    }
}