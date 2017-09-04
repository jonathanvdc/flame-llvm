using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A try-catch-finally block implementation for the Itanium C++ ABI.
    /// </summary>
    public sealed class ItaniumCxxTryBlock : CodeBlock
    {
        /// <summary>
        /// Creates a try-catch-finally block.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="TryBody">The body of the try clause.</param>
        /// <param name="FinallyBody">The body of the finally clause.</param>
        /// <param name="CatchClauses">The list of catch clauses.</param>
        public ItaniumCxxTryBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock TryBody,
            CodeBlock FinallyBody,
            IReadOnlyList<CatchClause> CatchClauses)
        {
            this.TryBody = TryBody;
            this.FinallyBody = FinallyBody;
            this.CatchClauses = CatchClauses;
        }

        /// <summary>
        /// Gets the body of the try clause in this block.
        /// </summary>
        /// <returns>The try clause's body.</returns>
        public CodeBlock TryBody { get; private set; }

        /// <summary>
        /// Gets the body of the finally clause in this block.
        /// </summary>
        /// <returns>The finally clause's body.</returns>
        public CodeBlock FinallyBody { get; private set; }

        /// <summary>
        /// Gets a read-only list of catch clauses in this block.
        /// </summary>
        /// <returns>The list of catch clauses.</returns>
        public IReadOnlyList<CatchClause> CatchClauses { get; private set; }

        private ICodeGenerator codeGenerator;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGenerator;

        /// <inheritdoc/>
        public override IType Type => TryBody.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var catchBlock = BasicBlock.CreateChildBlock("catch");
            var finallyBlock = BasicBlock.CreateChildBlock("finally");
            var leaveBlock = BasicBlock.CreateChildBlock("leave");

            // The try block is a regular block that jumps to the finally block.
            //
            // try:
            //     <try body>
            //     goto finally;

            var tryCodegen = TryBody.Emit(BasicBlock.WithUnwindTarget(catchBlock));
            BuildBr(tryCodegen.BasicBlock.Builder, finallyBlock.Block);

            // The finally block is just a normal block that does this:
            //
            // finally:
            //     <finally body>
            //     if (exception == null)
            //     {
            //         goto leave_try;
            //     }
            //     else
            //     {
            //         static if (is_top_level_try)
            //             resume exception;
            //         else
            //             goto next_unwind_target;
            //     }

            var finallyTail = FinallyBody.Emit(finallyBlock).BasicBlock;
            var exceptionTuple = BuildLoad(
                finallyTail.Builder,
                finallyBlock.FunctionBody.ExceptionTupleStorage.Value,
                "exception_tuple");
            var exceptionVal = BuildExtractValue(
                finallyTail.Builder,
                exceptionTuple,
                0,
                "exception_val");

            LLVMBasicBlockRef propagateExceptionBlock;
            if (BasicBlock.HasUnwindTarget)
            {
                propagateExceptionBlock = BasicBlock.UnwindTarget;
            }
            else
            {
                var resumeBlock = finallyTail.CreateChildBlock("resume");
                BuildResume(resumeBlock.Builder, exceptionTuple);
                propagateExceptionBlock = resumeBlock.Block;
            }

            BuildCondBr(
                finallyTail.Builder,
                BuildICmp(
                    finallyTail.Builder,
                    LLVMIntPredicate.LLVMIntEQ,
                    exceptionVal,
                    ConstNull(exceptionVal.TypeOf()),
                    "has_no_exception"),
                leaveBlock.Block,
                propagateExceptionBlock);

            throw new System.NotImplementedException();
        }
    }
}