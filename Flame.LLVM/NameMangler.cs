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
        /// <returns>The mangled name.</returns>
        public abstract string Mangle(IMethod Method);
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
        public override string Mangle(IMethod Method)
        {
            return Method.Name.ToString();
        }
    }
}