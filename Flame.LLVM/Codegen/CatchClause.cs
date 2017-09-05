using Flame.Compiler.Emit;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// Implements a simple catch header.
    /// </summary>
    public sealed class CatchHeader : ICatchHeader
    {
        public CatchHeader(IType ExceptionType, IEmitVariable ExceptionVariable)
        {
            this.ExceptionType = ExceptionType;
            this.ExceptionVariable = ExceptionVariable;
        }

        /// <summary>
        /// Gets the catch header's exception type.
        /// </summary>
        /// <returns>The catch header's exception type.</returns>
        public IType ExceptionType { get; private set; }

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
        public CatchClause(CatchHeader Header, CodeBlock Body)
        {
            this.header = Header;
            this.Body = Body;
        }

        private CatchHeader header;

        /// <summary>
        /// Gets the catch header for this clause.
        /// </summary>
        /// <returns>The catch header.</returns>
        public ICatchHeader Header => header;

        /// <summary>
        /// Gets the catch clause's exception type.
        /// </summary>
        /// <returns>The catch clause's exception type.</returns>
        public IType ExceptionType => header.ExceptionType;

        /// <summary>
        /// Gets this catch clause's body.
        /// </summary>
        /// <returns>The catch clause body.</returns>
        public CodeBlock Body { get; private set; }
    }
}