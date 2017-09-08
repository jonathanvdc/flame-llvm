using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A try-finally block implementation for the Itanium C++ ABI.
    /// </summary>
    public sealed class ItaniumCxxFinallyBlock : CodeBlock
    {
        /// <summary>
        /// Creates a try-finally block.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="TryBody">The body of the try clause.</param>
        /// <param name="FinallyBody">The body of the finally clause.</param>
        public ItaniumCxxFinallyBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock TryBody,
            CodeBlock FinallyBody)
        {
            this.codeGenerator = CodeGenerator;
            this.TryBody = TryBody;
            this.FinallyBody = FinallyBody;
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

        private ICodeGenerator codeGenerator;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGenerator;

        /// <inheritdoc/>
        public override IType Type => TryBody.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var exceptionDataType = StructType(new[] { PointerType(Int8Type(), 0), Int32Type() }, false);
            var isPropagatingStorage = BasicBlock.FunctionBody.CreateEntryPointAlloca(
                Int1Type(),
                "exception_value_alloca");

            var finallyBlock = BasicBlock.CreateChildBlock("finally");
            var finallyLandingPadBlock = BasicBlock.CreateChildBlock("finally_landingpad");
            var finallyExceptionBlock = BasicBlock.CreateChildBlock("finally_exception");
            var leaveBlock = BasicBlock.CreateChildBlock("leave");

            // The try block is a regular block that jumps to the finally block.
            //
            // try:
            //     store i1 false, i1* %is_propagating_exception_alloca
            //     <try body>
            //     br label %finally

            BuildStore(
                BasicBlock.Builder,
                ConstInt(Int1Type(), 0, false),
                isPropagatingStorage);
            var tryCodegen = TryBody.Emit(BasicBlock.WithUnwindTarget(finallyLandingPadBlock, finallyExceptionBlock));
            BuildBr(tryCodegen.BasicBlock.Builder, finallyBlock.Block);

            PopulateFinallyBlock(finallyBlock, leaveBlock, isPropagatingStorage);
            PopulateThunkLandingPadBlock(finallyLandingPadBlock, finallyExceptionBlock.Block, true);

            // The 'finally_exception' block is entered if an exception is propagating from
            // the 'try' block. It sets the 'is propagating' flag to 'true' and branches
            // to the finally block.
            //
            // finally_exception:
            //     store i1 true, i1* %is_propagating_exception_alloca
            //     br label %finally

            BuildStore(
                finallyExceptionBlock.Builder,
                ConstInt(Int1Type(), 1, false),
                isPropagatingStorage);
            BuildBr(finallyExceptionBlock.Builder, finallyBlock.Block);

            return new BlockCodegen(leaveBlock, tryCodegen.Value);
        }

        /// <summary>
        /// Populates a 'finally' block with instructions.
        /// </summary>
        /// <param name="FinallyBlock">The 'finally' block to populate.</param>
        /// <param name="LeaveBlock">
        /// The 'leave' block to which the 'finally' block jumps if no exception was thrown.
        /// </param>
        /// <param name="IsPropagatingStorage">
        /// A pointer to a Boolean flag that tells if an exception is being propagated.
        /// </param>
        private void PopulateFinallyBlock(
            BasicBlockBuilder FinallyBlock,
            BasicBlockBuilder LeaveBlock,
            LLVMValueRef IsPropagatingStorage)
        {
            // A finally block is just a normal block that does this:
            //
            // finally:
            //     <finally body>
            //     if (is_handling_exception)
            //     {
            //         unwind;
            //     }
            //     else
            //     {
            //         goto leave_try;
            //     }

            var finallyTail = FinallyBody.Emit(FinallyBlock).BasicBlock;
            var propagateExceptionBlock = GetManualUnwindTarget(finallyTail);

            BuildCondBr(
                finallyTail.Builder,
                BuildLoad(
                    finallyTail.Builder,
                    IsPropagatingStorage,
                    "is_handling_exception"),
                propagateExceptionBlock,
                LeaveBlock.Block);
        }

        /// <summary>
        /// Gets the manual unwind target for the given block, if any.
        /// If there is no manual unwind target, then a block is created
        /// that resumes the exception.
        /// </summary>
        /// <param name="BasicBlock">The basic block to unwind from.</param>
        /// <returns>An unwind target block.</returns>
        public static LLVMBasicBlockRef GetManualUnwindTarget(BasicBlockBuilder BasicBlock)
        {
            // The idea is to create a block that, if jumped to, does the following:
            //
            //     static if (is_top_level_try)
            //         resume exception_data;
            //     else
            //         goto next_unwind_target;

            if (BasicBlock.HasUnwindTarget)
            {
                return BasicBlock.ManualUnwindTarget;
            }
            else
            {
                var resumeBlock = BasicBlock.CreateChildBlock("resume");
                var exceptionTuple = BuildLoad(
                    resumeBlock.Builder,
                    resumeBlock.FunctionBody.ExceptionDataStorage.Value,
                    "exception_tuple");
                BuildResume(resumeBlock.Builder, exceptionTuple);
                return resumeBlock.Block;
            }
        }

        /// <summary>
        /// Populates a thunk landing pad block: a basic block that starts with a
        /// landing pad that catches any and all exceptions, stores the exception
        /// data and then jumps unconditionally to the target block.
        /// </summary>
        /// <param name="ThunkLandingPad">The thunk landing pad block to set up.</param>
        /// <param name="Target">The thunk landing pad block's target.</param>
        /// <param name="IsCleanup">
        /// Specifies if the thunk should use a 'cleanup' landing pad or a catch-all landing pad.
        /// </param>
        public static void PopulateThunkLandingPadBlock(
            BasicBlockBuilder ThunkLandingPad,
            LLVMBasicBlockRef Target,
            bool IsCleanup)
        {
            // A thunk landing pad block is an unwind target that does little more than
            // branch to its target.
            //
            // thunk:
            //     %exception_data = { i8*, i32 } landingpad (cleanup|catch i8* null)
            //     store { i8*, i32 } %exception_data, { i8*, i32 }* %exception_data_alloca
            //     br %target

            var bytePtr = PointerType(Int8Type(), 0);
            var exceptionDataType = StructType(new[] { bytePtr, Int32Type() }, false);
            var personality = ThunkLandingPad.FunctionBody.Module.Declare(IntrinsicValue.GxxPersonalityV0);

            var exceptionData = BuildLandingPad(
                ThunkLandingPad.Builder,
                exceptionDataType,
                ConstBitCast(personality, bytePtr),
                IsCleanup ? 0u : 1u,
                "exception_data");

            if (IsCleanup)
            {
                exceptionData.SetCleanup(true);
            }
            else
            {
                exceptionData.AddClause(ConstNull(bytePtr));
            }

            BuildStore(
                ThunkLandingPad.Builder,
                exceptionData,
                ThunkLandingPad.FunctionBody.ExceptionDataStorage.Value);

            BuildBr(ThunkLandingPad.Builder, Target);
        }
    }
}