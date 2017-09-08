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
            var exceptionDataType = StructType(new[] { PointerType(Int8Type(), 0), Int32Type() }, false);

            var finallyBlock = BasicBlock.CreateChildBlock("finally");
            var catchBlock = BasicBlock.CreateChildBlock("catch");
            var catchLandingPadBlock = BasicBlock.CreateChildBlock("catch_landingpad");
            var leaveBlock = BasicBlock.CreateChildBlock("leave");

            // The try block is a regular block that jumps to the finally block.
            //
            // try:
            //     store i8* null, i8** %exception_alloca
            //     <try body>
            //     goto finally;

            BuildStore(
                BasicBlock.Builder,
                ConstNull(PointerType(Int8Type(), 0)),
                BasicBlock.FunctionBody.ExceptionValueStorage.Value);
            var tryCodegen = TryBody.Emit(BasicBlock.WithUnwindTarget(catchLandingPadBlock, catchBlock));
            BuildBr(tryCodegen.BasicBlock.Builder, finallyBlock.Block);

            PopulateFinallyBlock(finallyBlock, leaveBlock);

            PopulateCatchBlock(catchBlock, finallyBlock);
            PopulateThunkLandingPadBlock(catchLandingPadBlock, catchBlock);

            return new BlockCodegen(leaveBlock, tryCodegen.Value);
        }

        /// <summary>
        /// Populates a 'finally' block with instructions.
        /// </summary>
        /// <param name="FinallyBlock">The 'finally' block to populate.</param>
        /// <param name="LeaveBlock">
        /// The 'leave' block to which the 'finally' block jumps if no exception was thrown.
        /// </param>
        private void PopulateFinallyBlock(
            BasicBlockBuilder FinallyBlock,
            BasicBlockBuilder LeaveBlock)
        {
            // A finally block is just a normal block that does this:
            //
            // finally:
            //     <finally body>
            //     if (exception_val == null)
            //     {
            //         goto leave_try;
            //     }
            //     else
            //     {
            //         static if (is_top_level_try)
            //             resume exception_data;
            //         else
            //             goto next_unwind_target;
            //     }

            var finallyTail = FinallyBody.Emit(FinallyBlock).BasicBlock;
            var exceptionVal = BuildLoad(
                finallyTail.Builder,
                finallyTail.FunctionBody.ExceptionValueStorage.Value,
                "exception_val");

            LLVMBasicBlockRef propagateExceptionBlock;
            if (FinallyBlock.HasUnwindTarget)
            {
                propagateExceptionBlock = FinallyBlock.ManualUnwindTarget;
            }
            else
            {
                var resumeBlock = finallyTail.CreateChildBlock("resume");
                var exceptionTuple = BuildLoad(
                    resumeBlock.Builder,
                    resumeBlock.FunctionBody.ExceptionDataStorage.Value,
                    "exception_tuple");
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
                LeaveBlock.Block,
                propagateExceptionBlock);
        }

        /// <summary>
        /// Populates a thunk landing pad.
        /// </summary>
        /// <param name="ThunkLandingPad">The thunk landing pad block to set up.</param>
        /// <param name="Target">The thunk landing pad block's target.</param>
        private void PopulateThunkLandingPadBlock(
            BasicBlockBuilder ThunkLandingPad,
            BasicBlockBuilder Target)
        {
            // A thunk landing pad block is an unwind target that does little more than
            // branch to its target.
            //
            // thunk:
            //     %exception_data = { i8*, i32 } landingpad
            //         catch i8* null
            //     store { i8*, i32 } %exception_data, { i8*, i32 }* %exception_data_alloca
            //     br %target

            var bytePtr = PointerType(Int8Type(), 0);
            var exceptionDataType = StructType(new[] { bytePtr, Int32Type() }, false);
            var personality = Target.FunctionBody.Module.Declare(IntrinsicValue.GxxPersonalityV0);

            var exceptionData = BuildLandingPad(
                ThunkLandingPad.Builder,
                exceptionDataType,
                ConstBitCast(personality, bytePtr),
                1,
                "exception_data");
            exceptionData.AddClause(ConstNull(bytePtr));

            BuildStore(
                ThunkLandingPad.Builder,
                exceptionData,
                ThunkLandingPad.FunctionBody.ExceptionDataStorage.Value);

            BuildBr(ThunkLandingPad.Builder, Target.Block);
        }

        /// <summary>
        /// Populates the given 'catch' block.
        /// </summary>
        /// <param name="CatchBlock">The 'catch' block to populate.</param>
        /// <param name="FinallyBlock">The 'finally' block to jump to when the 'catch' is done.</param>
        private void PopulateCatchBlock(BasicBlockBuilder CatchBlock, BasicBlockBuilder FinallyBlock)
        {
            // Before we even get started on the catch block's body, we should first
            // take the time to ensure that the '__cxa_begin_catch' environment set up
            // by the catch block is properly terminated by a '__cxa_end_catch' call,
            // even if an exception is thrown from the catch block itself. We can do
            // so by creating a 'catch_end' block and a thunk landing pad.
            var catchEndBlock = CatchBlock.CreateChildBlock("catch_end");

            var catchEndLandingPadBlock = CatchBlock.CreateChildBlock("catch_end_landingpad");
            PopulateThunkLandingPadBlock(catchEndLandingPadBlock, catchEndBlock);

            CatchBlock = CatchBlock.WithUnwindTarget(catchEndLandingPadBlock, catchEndBlock);

            // The catch block starts like this:
            //
            // catch:
            //     %exception_data = load { i8*, i32 }* %exception_data_alloca
            //     %exception_obj = extractvalue { i8*, i32 } %exception_data, 0
            //     %exception_ptr_opaque = call i8* @__cxa_begin_catch(i8* %exception_obj)
            //     %exception_ptr = bitcast i8* %exception_ptr_opaque to i8**
            //     %exception = load i8*, i8** %exception_ptr
            //     store i8* %exception, i8** %exception_alloca
            //     %exception_vtable_ptr_ptr = bitcast i8* %exception to i8**
            //     %exception_vtable_ptr = load i8*, i8** %exception_vtable_ptr_ptr
            //     %exception_typeid = <typeid> i64, i8* %exception_vtable_ptr

            var exceptionData = BuildLoad(
                CatchBlock.Builder,
                CatchBlock.FunctionBody.ExceptionDataStorage.Value,
                "exception_data");

            var exceptionObj = BuildExtractValue(
                CatchBlock.Builder,
                exceptionData,
                0,
                "exception_obj");

            var exceptionPtrOpaque = BuildCall(
                CatchBlock.Builder,
                CatchBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaBeginCatch),
                new LLVMValueRef[] { exceptionObj },
                "exception_ptr_opaque");

            var exceptionPtr = BuildBitCast(
                CatchBlock.Builder,
                exceptionPtrOpaque,
                PointerType(PointerType(Int8Type(), 0), 0),
                "exception_ptr");

            var exception = BuildLoad(CatchBlock.Builder, exceptionPtr, "exception");

            BuildStore(
                CatchBlock.Builder,
                exception,
                CatchBlock.FunctionBody.ExceptionValueStorage.Value);

            var exceptionVtablePtrPtr = BuildBitCast(
                CatchBlock.Builder,
                exception,
                PointerType(PointerType(Int8Type(), 0), 0),
                "exception_vtable_ptr_ptr");

            var exceptionVtablePtr = AtAddressEmitVariable.BuildConstantLoad(
                CatchBlock.Builder,
                exceptionVtablePtrPtr,
                "exception_vtable_ptr");

            var exceptionTypeid = TypeIdBlock.BuildTypeid(
                CatchBlock.Builder,
                exceptionVtablePtr);

            // Next, we need to figure out if we have a catch block that can handle the
            // exception we've thrown. We do so by iterating over all catch blocks and
            // testing if they're a match for the exception's type.
            var fallthroughBlock = CatchBlock.CreateChildBlock("catch_test");
            for (int i = 0; i < CatchClauses.Count; i++)
            {
                var clause = CatchClauses[i];

                var catchBodyBlock = CatchBlock.CreateChildBlock("catch_body");
                var catchBodyBlockTail = catchBodyBlock;

                // First, emit the catch body. This is just a regular block that
                // clears the exception value variable when it gets started and
                // jumps to the 'catch_end' when it's done.
                //
                // catch_body:
                //     store i8* null, i8** %exception_alloca
                //     %exception_val = bitcast i8* %exception to <clause_type>
                //     store <clause_type> %exception_val, <clause_type>* %clause_exception_variable_alloca
                //     <catch clause body>
                //     br catch_end

                BuildStore(
                    catchBodyBlockTail.Builder,
                    ConstNull(PointerType(Int8Type(), 0)),
                    catchBodyBlockTail.FunctionBody.ExceptionValueStorage.Value);

                var ehVarAddressAndBlock = clause.LLVMHeader
                    .AtAddressExceptionVariable.Address.Emit(catchBodyBlockTail);
                catchBodyBlockTail = ehVarAddressAndBlock.BasicBlock;

                BuildStore(
                    catchBodyBlockTail.Builder,
                    BuildBitCast(
                        catchBodyBlockTail.Builder,
                        exception,
                        catchBodyBlockTail.FunctionBody.Module.Declare(clause.ExceptionType),
                        "exception_val"),
                    ehVarAddressAndBlock.Value);

                catchBodyBlockTail = clause.Body.Emit(catchBodyBlockTail).BasicBlock;

                BuildBr(catchBodyBlockTail.Builder, catchEndBlock.Block);

                // Each clause is implemented as:
                //
                // catch_clause:
                //     %clause_typeid = <typeid> i64, clause-exception-type
                //     %typeid_rem = urem i64 %exception_typeid_tmp, %clause_typeid
                //     %is_subtype = cmp eq i64 %typeid_rem, 0
                //     br i1 %is_subtype, label %catch_body, label %fallthrough
                //
                var clauseTypeid = CatchBlock.FunctionBody.Module.GetTypeId((LLVMType)clause.ExceptionType);

                var typeIdRem = BuildURem(
                    CatchBlock.Builder,
                    exceptionTypeid,
                    ConstInt(exceptionTypeid.TypeOf(), clauseTypeid, false),
                    "typeid_rem");
                
                var isSubtype = BuildICmp(
                    CatchBlock.Builder,
                    LLVMIntPredicate.LLVMIntEQ,
                    typeIdRem,
                    ConstInt(exceptionTypeid.TypeOf(), 0, false),
                    "is_subtype");

                BuildCondBr(CatchBlock.Builder, isSubtype, catchBodyBlock.Block, fallthroughBlock.Block);

                CatchBlock = fallthroughBlock;
                fallthroughBlock = CatchBlock.CreateChildBlock("catch_test");
            }

            // If we didn't match any catch clauses, then we'll just end the 'catch' block
            // and keep unwinding.
            BuildBr(CatchBlock.Builder, catchEndBlock.Block);
            BuildBr(fallthroughBlock.Builder, catchEndBlock.Block);

            // The catch end block simply calls '__cxa_end_catch' and jumps to the finally
            // block, i.e., the manual unwind target of the 'catch' block. Like so:
            //
            // catch_end:
            //     call void @__cxa_end_catch()
            //     br %finally

            BuildCall(
                catchEndBlock.Builder,
                CatchBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaEndCatch),
                new LLVMValueRef[] { },
                "");
            BuildBr(catchEndBlock.Builder, FinallyBlock.Block);
        }
    }
}