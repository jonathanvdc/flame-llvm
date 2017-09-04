﻿using System;
using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A base class for LLVM code blocks.
    /// </summary>
    public abstract class CodeBlock : ICodeBlock
    {
        /// <summary>
        /// Gets the code generator for this code block.
        /// </summary>
        /// <returns>The code generator.</returns>
        public abstract ICodeGenerator CodeGenerator { get; }

        /// <summary>
        /// Gets the type of value produced by this code block.
        /// </summary>
        /// <returns>This code block's type.</returns>
        public abstract IType Type { get; }

        /// <summary>
        /// Emits this LLVM code block's contents to the given basic block.
        /// </summary>
        /// <param name="BasicBlock">The basic block to extend.</param>
        /// <returns>The next basic block to generate code for.</returns>
        public abstract BlockCodegen Emit(BasicBlockBuilder BasicBlock);
    }

    /// <summary>
    /// Represents the code generated by an LLVM code block.
    /// </summary>
    public struct BlockCodegen
    {
        public BlockCodegen(BasicBlockBuilder BasicBlock, LLVMValueRef Value)
        {
            this.BasicBlock = BasicBlock;
            this.Value = Value;
        }

        public BlockCodegen(BasicBlockBuilder BasicBlock)
        {
            this.BasicBlock = BasicBlock;
            this.Value = default(LLVMValueRef);
        }

        /// <summary>
        /// Gets the next basic block to generate code for.
        /// </summary>
        /// <returns>The next basic block to generate code for.</returns>
        public BasicBlockBuilder BasicBlock { get; private set; }

        /// <summary>
        /// Gets the value that computes a code block's result.
        /// </summary>
        /// <returns>The code block's result.</returns>
        public LLVMValueRef Value { get; private set; }

        /// <summary>
        /// Tells if the codegen for this block produces a value.
        /// </summary>
        public bool HasValue => Value.Pointer != IntPtr.Zero;
    }

    /// <summary>
    /// A data structure that makes building LLVM function bodies easier.
    /// </summary>
    public sealed class FunctionBodyBuilder
    {
        public FunctionBodyBuilder(
            LLVMModuleBuilder Module,
            LLVMValueRef Function)
        {
            this.Module = Module;
            this.Function = Function;
            this.blockNameSet = new UniqueNameSet<string>(Id, "block_");
            this.taggedValues = new Dictionary<UniqueTag, LLVMValueRef>();
            this.breakBlocks = new Dictionary<UniqueTag, LLVMBasicBlockRef>();
            this.continueBlocks = new Dictionary<UniqueTag, LLVMBasicBlockRef>();
            this.ExceptionDataStorage = new Lazy<LLVMValueRef>(CreateExceptionDataStorage);
            this.ExceptionValueStorage = new Lazy<LLVMValueRef>(CreateExceptionValueStorage);
        }

        /// <summary>
        /// Gets the module builder for the module that defines the
        /// function under construction.
        /// </summary>
        /// <returns>The module builder.</returns>
        public LLVMModuleBuilder Module { get; private set; }

        /// <summary>
        /// Gets the LLVM function that is under construction.
        /// </summary>
        /// <returns>The LLVM function.</returns>
        public LLVMValueRef Function { get; private set; }

        /// <summary>
        /// Gets the storage location for the current exception's data.
        /// </summary>
        /// <returns>The storage location for the current exception's data.</returns>
        public Lazy<LLVMValueRef> ExceptionDataStorage { get; private set; }

        /// <summary>
        /// Gets the storage location for the current exception's value.
        /// </summary>
        /// <returns>The storage location for the current exception's value.</returns>
        public Lazy<LLVMValueRef> ExceptionValueStorage { get; private set; }

        private Dictionary<UniqueTag, LLVMValueRef> taggedValues;
        private Dictionary<UniqueTag, LLVMBasicBlockRef> breakBlocks;
        private Dictionary<UniqueTag, LLVMBasicBlockRef> continueBlocks;

        private UniqueNameSet<string> blockNameSet;

        private string Id(string Value)
        {
            return Value;
        }

        private LLVMValueRef CreateEntryPointAlloca(LLVMTypeRef Type, string Name)
        {
            var entry = Function.GetEntryBasicBlock();
            var builder = CreateBuilder();
            PositionBuilderBefore(builder, entry.GetFirstInstruction());
            var result = BuildAlloca(builder, Type, Name);
            DisposeBuilder(builder);
            return result;
        }

        private LLVMValueRef CreateExceptionDataStorage()
        {
            return CreateEntryPointAlloca(
                StructType(new[] { PointerType(Int8Type(), 0), Int32Type() }, false),
                "exception_tuple_alloca");
        }

        private LLVMValueRef CreateExceptionValueStorage()
        {
            return CreateEntryPointAlloca(
                PointerType(Int8Type(), 0),
                "exception_value_alloca");
        }

        /// <summary>
        /// Tags the given value with the given tag.
        /// </summary>
        /// <param name="Tag">The tag for the value.</param>
        /// <param name="Value">The value to tag.</param>
        public void TagValue(UniqueTag Tag, LLVMValueRef Value)
        {
            taggedValues.Add(Tag, Value);
        }

        /// <summary>
        /// Gets the tagged value with the given tag.
        /// </summary>
        /// <param name="Tag">The tag of the value to retrieve.</param>
        /// <returns>The tagged value.</returns>
        public LLVMValueRef GetTaggedValue(UniqueTag Tag)
        {
            return taggedValues[Tag];
        }

        /// <summary>
        /// Tags the given break and continue blocks with the given tag.
        /// </summary>
        /// <param name="Tag">The tag for a flow block.</param>
        /// <param name="BreakBlock">The 'break' basic block for the flow block.</param>
        /// <param name="ContinueBlock">The 'continue' basic block for the flow block.</param>
        public void TagFlowBlock(
            UniqueTag Tag,
            LLVMBasicBlockRef BreakBlock,
            LLVMBasicBlockRef ContinueBlock)
        {
            breakBlocks.Add(Tag, BreakBlock);
            continueBlocks.Add(Tag, ContinueBlock);
        }

        /// <summary>
        /// Gets the 'break' basic block for the given tag.
        /// </summary>
        /// <param name="Tag">The tag to find a 'break' basic block for.</param>
        /// <returns>The 'break' basic block.</returns>
        public LLVMBasicBlockRef GetBreakBlock(UniqueTag Tag)
        {
            return breakBlocks[Tag];
        }

        /// <summary>
        /// Gets the 'continue' basic block for the given tag.
        /// </summary>
        /// <param name="Tag">The tag to find a 'continue' basic block for.</param>
        /// <returns>The 'continue' basic block.</returns>
        public LLVMBasicBlockRef GetContinueBlock(UniqueTag Tag)
        {
            return continueBlocks[Tag];
        }

        /// <summary>
        /// Appends a basic block to this function body.
        /// </summary>
        /// <param name="Name">The preferred name for the basic block.</param>
        /// <returns>A basic block builder.</returns>
        public BasicBlockBuilder AppendBasicBlock(string Name)
        {
            var basicBlock = LLVMSharp.LLVM.AppendBasicBlock(
                Function,
                blockNameSet.GenerateName(Name));
            return new BasicBlockBuilder(this, basicBlock);
        }

        /// <summary>
        /// Appends a basic block to this function body.
        /// </summary>
        /// <returns>A basic block builder.</returns>
        public BasicBlockBuilder AppendBasicBlock()
        {
            return AppendBasicBlock("block_0");
        }
    }

    /// <summary>
    /// A data structure that makes building LLVM basic blocks easier.
    /// </summary>
    public sealed class BasicBlockBuilder
    {
        /// <summary>
        /// Creates a basic block builder from the given arguments.
        /// </summary>
        public BasicBlockBuilder(
            FunctionBodyBuilder FunctionBody,
            LLVMBasicBlockRef Block)
        {
            this.FunctionBody = FunctionBody;
            this.Block = Block;
            this.Builder = CreateBuilder();
            PositionBuilderAtEnd(Builder, Block);
        }

        ~BasicBlockBuilder()
        {
            DisposeBuilder(Builder);
        }

        /// <summary>
        /// Gets the function body that defines this basic block.
        /// </summary>
        /// <returns>The function body.</returns>
        public FunctionBodyBuilder FunctionBody { get; private set; }

        /// <summary>
        /// Gets the LLVM basic block that is under construction.
        /// </summary>
        /// <returns>The LLVM basic block.</returns>
        public LLVMBasicBlockRef Block { get; private set; }

        /// <summary>
        /// Gets the LLVM builder that is used to build the basic block.
        /// </summary>
        /// <returns>The LLVM builder for the basic block.</returns>
        public LLVMBuilderRef Builder { get; private set; }

        /// <summary>
        /// Gets the basic block to which 'invoke' instructions in this block
        /// should unwind.
        /// </summary>
        /// <returns>The unwind target.</returns>
        public LLVMBasicBlockRef UnwindTarget { get; private set; }

        /// <summary>
        /// Tests if this code block has an unwind target.
        /// </summary>
        public bool HasUnwindTarget => UnwindTarget.Pointer != IntPtr.Zero;

        /// <summary>
        /// Creates a child block from this basic block.
        /// </summary>
        /// <param name="Name">The child block's name.</param>
        /// <returns>A child block.</returns>
        public BasicBlockBuilder CreateChildBlock(string Name)
        {
            return FunctionBody.AppendBasicBlock(Name).WithUnwindTarget(UnwindTarget);
        }

        /// <summary>
        /// Creates a child block from this basic block.
        /// </summary>
        /// <returns>A child block.</returns>
        public BasicBlockBuilder CreateChildBlock()
        {
            return FunctionBody.AppendBasicBlock().WithUnwindTarget(UnwindTarget);
        }

        /// <summary>
        /// Creates a new basic block builder that appends instructions to
        /// this block, but with a different unwind target.
        /// </summary>
        /// <param name="Target">The new unwind target.</param>
        /// <returns>A new basic block builder for the same basic block.</returns>
        public BasicBlockBuilder WithUnwindTarget(LLVMBasicBlockRef Target)
        {
            var result = new BasicBlockBuilder(FunctionBody, Block);
            result.UnwindTarget = Target;
            return result;
        }

        /// <summary>
        /// Creates a new basic block builder that appends instructions to
        /// this block, but with a different unwind target.
        /// </summary>
        /// <param name="Target">The new unwind target.</param>
        /// <returns>A new basic block builder for the same basic block.</returns>
        public BasicBlockBuilder WithUnwindTarget(BasicBlockBuilder Target)
        {
            return WithUnwindTarget(Target.Block);
        }
    }
}

