using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that 
    /// </summary>
    public sealed class IfElseBlock : CodeBlock
    {
        /// <summary>
        /// Creates an if-else block from the given condition,
        /// if-clause and else-clause.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Condition">The condition.</param>
        /// <param name="IfClause">The if-clause.</param>
        /// <param name="ElseClause">The else-clause.</param>
        public IfElseBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Condition,
            CodeBlock IfClause,
            CodeBlock ElseClause)
        {
            this.codeGen = CodeGenerator;
            this.Condition = Condition;
            this.IfClause = IfClause;
            this.ElseClause = ElseClause;
        }

        /// <summary>
        /// Gets the condition this if-else block uses to decide
        /// which clause to run.
        /// </summary>
        /// <returns>The condition.</returns>
        public CodeBlock Condition { get; private set; }

        /// <summary>
        /// Gets the block of code that is executed if the condition
        /// evaluates to <c>true</c>.
        /// </summary>
        /// <returns>The if-clause.</returns>
        public CodeBlock IfClause { get; private set; }

        /// <summary>
        /// Gets the block of code that is executed if the condition
        /// evaluates to <c>false</c>.
        /// </summary>
        /// <returns>The else-clause.</returns>
        public CodeBlock ElseClause { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => IfClause.Type;

        private BlockCodegen EmitClause(
            BasicBlockBuilder MergeBlock,
            CodeBlock Body,
            string Name)
        {
            if (Body != null)
            {
                var clauseBlock = MergeBlock.FunctionBody.AppendBasicBlock(Name);
                var clauseResult = Body.Emit(clauseBlock);
                BuildBr(clauseResult.BasicBlock.Builder, MergeBlock.Block);
                return new BlockCodegen(clauseBlock, clauseResult.Value);
            }
            else
            {
                return new BlockCodegen(MergeBlock);
            }
        }

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var conditionResult = Condition.Emit(BasicBlock);
            BasicBlock = conditionResult.BasicBlock;
            var mergeBlock = BasicBlock.FunctionBody.AppendBasicBlock("if_else_merge");

            var ifResult = EmitClause(mergeBlock, IfClause, "if_clause");
            var elseResult = EmitClause(mergeBlock, ElseClause, "else_clause");
            BuildCondBr(
                BasicBlock.Builder,
                conditionResult.Value,
                ifResult.BasicBlock.Block,
                elseResult.BasicBlock.Block);

            if (ifResult.HasValue)
            {
                var phiVal = BuildPhi(mergeBlock.Builder, ifResult.Value.TypeOf(), "if_else_result");
                AddIncoming(
                    phiVal,
                    new LLVMValueRef[] { ifResult.Value, elseResult.Value },
                    new LLVMBasicBlockRef[]
                    {
                        ifResult.Value.GetInstructionParent(),
                        elseResult.Value.GetInstructionParent()
                    },
                    2);
                return new BlockCodegen(mergeBlock, phiVal);
            }
            else
            {
                return new BlockCodegen(mergeBlock);
            }
        }
    }
}