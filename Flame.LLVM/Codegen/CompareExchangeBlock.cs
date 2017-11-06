using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block type that compares two integers or pointers for equality and,
    /// if they are equal, replaces the first value.
    /// </summary>
    public sealed class CompareExchangeBlock : CodeBlock
    {
        public CompareExchangeBlock(
            CodeBlock Destination,
            CodeBlock Value,
            CodeBlock Comparand,
            LLVMCodeGenerator CodeGenerator)
        {
            this.Destination = Destination;
            this.Value = Value;
            this.Comparand = Comparand;
        }

        private LLVMCodeGenerator codeGen; 

        /// <summary>
        /// Gets a reference to the destination, whose value is compared with
        /// the comparand and possibly replaced.
        /// </summary>
        /// <returns>The destination address.</returns>
        public CodeBlock Destination { get; private set; }

        /// <summary>
        /// Gets the value that replaces the destination value if the comparison
        /// results in equality.
        /// </summary>
        /// <returns>The value.</returns>
        public CodeBlock Value { get; private set; }

        /// <summary>
        /// Gets the value that is compared to the value at the destination.
        /// </summary>
        /// <returns>The comparand.</returns>
        public CodeBlock Comparand { get; private set; }

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => Value.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var destCodegen = Destination.Emit(BasicBlock);
            BasicBlock = destCodegen.BasicBlock;
            var valCodegen = Value.Emit(BasicBlock);
            BasicBlock = valCodegen.BasicBlock;
            var comparandCodegen = Comparand.Emit(BasicBlock);
            BasicBlock = comparandCodegen.BasicBlock;

            var cmpxchg = BuildAtomicCmpXchg(
                BasicBlock.Builder,
                destCodegen.Value,
                comparandCodegen.Value,
                valCodegen.Value,
                LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                false);

            cmpxchg.SetValueName("atomic_cmpxchg");

            return new BlockCodegen(
                BasicBlock,
                BuildExtractValue(BasicBlock.Builder, cmpxchg, 0, "old_value"));
        }
    }
}