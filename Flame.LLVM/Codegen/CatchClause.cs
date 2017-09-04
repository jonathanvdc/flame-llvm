using Flame.Compiler.Emit;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// Implements a simple catch header.
    /// </summary>
    public sealed class CatchHeader : ICatchHeader
    {
        public CatchHeader(IEmitVariable ExceptionVariable)
        {
            this.ExceptionVariable = ExceptionVariable;
        }

        /// <summary>
        /// Gets the catch header's exception variable.
        /// </summary>
        /// <returns>The catch header's exception variable.</returns>
        public IEmitVariable ExceptionVariable { get; private set; }
    }

    /// <summary>
    /// Implements a simple catch clause.
    /// </summary>
    public sealed class CatchClause : ICatchClause
    {
        public CatchClause(ICatchHeader Header, CodeBlock Body)
        {
            this.Header = Header;
            this.Body = Body;
        }

        /// <summary>
        /// Gets the catch header for this clause.
        /// </summary>
        /// <returns>The catch header.</returns>
        public ICatchHeader Header { get; private set; }

        /// <summary>
        /// Gets this catch clause's body.
        /// </summary>
        /// <returns>The catch clause body.</returns>
        public CodeBlock Body { get; private set; }
    }
}