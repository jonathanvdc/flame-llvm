using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that stores a value at an address.
    /// </summary>
    public sealed class StoreBlock : CodeBlock
    {
        public StoreBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Address,
            CodeBlock Value)
        {
            this.codeGen = CodeGenerator;
            this.address = Address;
            this.val = Value;
        }

        private ICodeGenerator codeGen;
        private CodeBlock address;
        private CodeBlock val;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Void;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var lhsCodegen = address.Emit(BasicBlock);
            var rhsCodegen = val.Emit(lhsCodegen.BasicBlock);
            BuildStore(rhsCodegen.BasicBlock.Builder, rhsCodegen.Value, lhsCodegen.Value);
            return new BlockCodegen(rhsCodegen.BasicBlock);
        }
    }
}

