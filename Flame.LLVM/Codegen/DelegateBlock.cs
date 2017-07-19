using System;
using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that creates a delegate.
    /// </summary>
    public sealed class DelegateBlock : CodeBlock
    {
        /// <summary>
        /// Creates a delegate block from the given callee, target block
        /// and operator.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Callee">The callee.</param>
        /// <param name="Target">The target on which the callee is invoked.</param>
        /// <param name="Op">The type of delegate to create.</param>
        public DelegateBlock(
            ICodeGenerator CodeGenerator,
            IMethod Callee,
            CodeBlock Target,
            Operator Op)
        {
            this.Callee = Callee;
            this.Target = Target;
            this.Op = Op;
        }

        /// <summary>
        /// Gets the callee of the delegate to create.
        /// </summary>
        /// <returns>The callee.</returns>
        public IMethod Callee { get; private set; }

        /// <summary>
        /// Gets a code block that produces the target on which the callee
        /// is invoked by the delegate.
        /// </summary>
        /// <returns>The target on which the callee is invoked.</returns>
        public CodeBlock Target { get; private set; }

        /// <summary>
        /// Gets an operator that describes the type of delegate to create.
        /// </summary>
        /// <returns>The type of delegate to create.</returns>
        public Operator Op { get; private set; }

        /// <inheritdoc/>
        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => MethodType.Create(Callee);

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            throw new NotImplementedException();
        }
    }
}