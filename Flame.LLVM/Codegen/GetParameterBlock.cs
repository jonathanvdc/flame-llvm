using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that retrieves an LLVM function parameter.
    /// </summary>
    public sealed class GetParameterBlock : CodeBlock
    {
        public GetParameterBlock(
            ICodeGenerator CodeGenerator,
            int Index,
            IType ParameterType)
        {
            this.codeGen = CodeGenerator;
            this.Index = Index;
            this.paramType = ParameterType;
        }

        private ICodeGenerator codeGen;

        private IType paramType;

        /// <summary>
        /// Gets the parameter's index in the LLVM function's parameter list.
        /// </summary>
        /// <returns>The parameter index.</returns>
        public int Index { get; private set; }

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => paramType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(BasicBlock, GetParam(BasicBlock.FunctionBody.Function, (uint)Index));
        }
    }
}