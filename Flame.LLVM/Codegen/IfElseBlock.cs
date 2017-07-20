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
            string Name,
            out BasicBlockBuilder ClauseBlock)
        {
            if (Body != null)
            {
                ClauseBlock = MergeBlock.FunctionBody.AppendBasicBlock(Name);
                var clauseResult = Body.Emit(ClauseBlock);
                BuildBr(clauseResult.BasicBlock.Builder, MergeBlock.Block);
                return clauseResult;
            }
            else
            {
                ClauseBlock = MergeBlock;
                return new BlockCodegen(MergeBlock);
            }
        }

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var conditionResult = Condition.Emit(BasicBlock);
            BasicBlock = conditionResult.BasicBlock;
            var mergeBlock = BasicBlock.FunctionBody.AppendBasicBlock("if_else_merge");

            BasicBlockBuilder ifBlock;
            BasicBlockBuilder elseBlock;
            var ifResult = EmitClause(mergeBlock, IfClause, "if_clause", out ifBlock);
            var elseResult = EmitClause(mergeBlock, ElseClause, "else_clause", out elseBlock);
            BuildCondBr(
                BasicBlock.Builder,
                conditionResult.Value,
                ifBlock.Block,
                elseBlock.Block);

            if (ifResult.HasValue)
            {
                var phiVal = BuildPhi(mergeBlock.Builder, ifResult.Value.TypeOf(), "if_else_result");
                AddIncoming(
                    phiVal,
                    new LLVMValueRef[] { ifResult.Value, elseResult.Value },
                    new LLVMBasicBlockRef[]
                    {
                        ifResult.BasicBlock.Block,
                        elseResult.BasicBlock.Block
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