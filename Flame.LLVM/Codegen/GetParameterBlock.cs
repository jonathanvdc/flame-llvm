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
            var result = GetParam(BasicBlock.FunctionBody.Function, (uint)Index);
            var llvmType = BasicBlock.FunctionBody.Module.Declare(Type);
            if (result.TypeOf().Pointer != llvmType.Pointer)
            {
                result = BuildBitCast(BasicBlock.Builder, result, llvmType, "bitcast_tmp");
            }
            return new BlockCodegen(BasicBlock, result);
        }
    }
}