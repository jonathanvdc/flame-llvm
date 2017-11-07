using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    public sealed class ReadModifyWriteBlock : CodeBlock
    {
        public ReadModifyWriteBlock(
            CodeBlock DestinationAddress,
            CodeBlock Value,
            LLVMAtomicRMWBinOp Op,
            LLVMCodeGenerator CodeGenerator)
        {
            this.DestinationAddress = DestinationAddress;
            this.Value = Value;
            this.Op = Op;
            this.codeGen = CodeGenerator;
        }

        /// <summary>
        /// Gets the reference to a destination, whose value is first read from
        /// and then written to in an atomic operation.
        /// </summary>
        /// <returns>The destination address.</returns>
        public CodeBlock DestinationAddress { get; private set; }

        /// <summary>
        /// Gets the right-hand side of this binary operation.
        /// </summary>
        /// <returns>The right-hand side.</returns>
        public CodeBlock Value { get; private set; }

        /// <summary>
        /// Gets the binary operation to apply.
        /// </summary>
        /// <returns>The binary operation.</returns>
        public LLVMAtomicRMWBinOp Op { get; private set; }

        private LLVMCodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => Value.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var lhsAddrCodegen = DestinationAddress.Emit(BasicBlock);
            BasicBlock = lhsAddrCodegen.BasicBlock;
            var rhsValueCodegen = Value.Emit(BasicBlock);
            BasicBlock = rhsValueCodegen.BasicBlock;

            // LLVM AtomicRMW instructions always return the *old* value,
            // but we want the *new* value (with Xchg being the lone
            // exception).
            //
            // We can compute the new value by first performing the RMW
            // and then computing the new value ourselves.

            var rmwOp = BuildAtomicRMW(
                BasicBlock.Builder,
                Op,
                lhsAddrCodegen.Value,
                rhsValueCodegen.Value,
                LLVMAtomicOrdering.LLVMAtomicOrderingSequentiallyConsistent,
                false);

            return new BlockCodegen(
                BasicBlock,
                BuildRMWResult(
                    BasicBlock.Builder,
                    Op,
                    rmwOp,
                    rhsValueCodegen.Value));
        }

        /// <summary>
        /// Builds an instruction that computes the new value of a destination
        /// address after applying an RMW operator.
        /// </summary>
        /// <param name="Builder">A basic block builder.</param>
        /// <param name="Operator">The RMW binary operator to apply.</param>
        /// <param name="Left">The value in the destination.</param>
        /// <param name="Right">The second operand value.</param>
        /// <returns>The new value of the destination address.</returns>
        public static LLVMValueRef BuildRMWResult(
            LLVMBuilderRef Builder,
            LLVMAtomicRMWBinOp Operator,
            LLVMValueRef Left, LLVMValueRef Right)
        {
            switch (Operator)
            {
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd:
                    return BuildAdd(Builder, Left, Right, "rmw_result");
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd:
                    return BuildAnd(Builder, Left, Right, "rmw_result");
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr:
                    return BuildOr(Builder, Left, Right, "rmw_result");
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub:
                    return BuildSub(Builder, Left, Right, "rmw_result");
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg:
                    return Left;
                case LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXor:
                    return BuildXor(Builder, Left, Right, "rmw_result");
                default:
                    throw new NotSupportedException("Unsupported RMW operator: " + Operator);
            }
        }

        /// <summary>
        /// Parses the atomic RMW binary operator with the given name.
        /// </summary>
        /// <param name="Name">The name of the operator to parse.</param>
        /// <returns>An atomic RMW binary operator.</returns>
        public static LLVMAtomicRMWBinOp ParseOperator(string Name)
        {
            switch (Name.ToLowerInvariant())
            {
                case "add":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAdd;
                case "and":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpAnd;
                case "or":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpOr;
                case "sub":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpSub;
                case "xchg":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXchg;
                case "xor":
                    return LLVMAtomicRMWBinOp.LLVMAtomicRMWBinOpXor;
                default:
                    throw new NotSupportedException("Unsupported RMW operator: " + Name);
            }
        }
    }
}