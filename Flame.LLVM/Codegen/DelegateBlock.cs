using System;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that creates a delegate.
    /// </summary>
    public sealed class DelegateBlock : CodeBlock
    {
        /// <summary>
        /// Creates a delegate block from the given callee, target block
        /// and operator.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Callee">The callee.</param>
        /// <param name="Target">The target on which the callee is invoked.</param>
        /// <param name="Op">The type of delegate to create.</param>
        public DelegateBlock(
            LLVMCodeGenerator CodeGenerator,
            IMethod Callee,
            CodeBlock Target,
            Operator Op)
        {
            this.codeGen = CodeGenerator;
            this.Callee = Callee;
            this.Target = Target;
            this.Op = Op;
        }

        /// <summary>
        /// Gets the callee of the delegate to create.
        /// </summary>
        /// <returns>The callee.</returns>
        public IMethod Callee { get; private set; }

        /// <summary>
        /// Gets a code block that produces the target on which the callee
        /// is invoked by the delegate.
        /// </summary>
        /// <returns>The target on which the callee is invoked.</returns>
        public CodeBlock Target { get; private set; }

        /// <summary>
        /// Gets an operator that describes the type of delegate to create.
        /// </summary>
        /// <returns>The type of delegate to create.</returns>
        public Operator Op { get; private set; }

        /// <inheritdoc/>
        private LLVMCodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => MethodType.Create(Callee);

        /// <summary>
        /// The data layout of a method type.
        /// </summary>
        public static readonly LLVMTypeRef MethodTypeLayout =
            StructType(
                new LLVMTypeRef[]
                {
                    PointerType(Int8Type(), 0), // vtable pointer
                    PointerType(Int8Type(), 0), // context pointer
                    PointerType(Int8Type(), 0), // function pointer
                    Int1Type() // context?
                },
                false);

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var targetAndBlock = InvocationBlock.EmitTarget(BasicBlock, Target);
            BasicBlock = targetAndBlock.BasicBlock;

            var calleeAndBlock = InvocationBlock.EmitCallee(BasicBlock, targetAndBlock.Value, Callee, Op);
            BasicBlock = calleeAndBlock.BasicBlock;

            var methodTypeAllocBlock = (CodeBlock)codeGen.Allocate(
                new StaticCastExpression(
                    LLVMCodeGenerator.ToExpression(
                        new ConstantBlock(codeGen, PrimitiveTypes.UInt64, SizeOf(MethodTypeLayout))),
                    PrimitiveTypes.UInt64),
                Type).Emit(codeGen);

            var methodTypeAllocCodegen = methodTypeAllocBlock.Emit(BasicBlock);
            BasicBlock = methodTypeAllocCodegen.BasicBlock;

            BuildStore(
                BasicBlock.Builder,
                TypeVTableBlock.BuildTypeVTable(BasicBlock, Type),
                BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 0, "vtable_ptr_ptr"));

            bool hasContext = targetAndBlock.Value.Pointer != IntPtr.Zero;
            if (hasContext)
            {
                BuildStore(
                    BasicBlock.Builder,
                    BuildBitCast(
                        BasicBlock.Builder,
                        targetAndBlock.Value,
                        PointerType(Int8Type(), 0),
                        "context_obj"),
                    BuildStructGEP(
                        BasicBlock.Builder,
                        methodTypeAllocCodegen.Value,
                        1,
                        "context_obj_ptr"));
            }

            BuildStore(
                BasicBlock.Builder,
                BuildBitCast(
                    BasicBlock.Builder,
                    calleeAndBlock.Value,
                    PointerType(Int8Type(), 0),
                    "opaque_func_ptr"),
                BuildStructGEP(
                    BasicBlock.Builder,
                    methodTypeAllocCodegen.Value,
                    2,
                    "func_ptr_ptr"));

            BuildStore(
                BasicBlock.Builder,
                ConstInt(Int1Type(), hasContext ? 1ul : 0ul, false),
                BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 3, "has_context_ptr"));

            return new BlockCodegen(BasicBlock, methodTypeAllocCodegen.Value);
        }

        private static LLVMValueRef BuildLoadFieldPointer(
            LLVMBuilderRef Builder,
            LLVMValueRef Delegate,
            uint FieldIndex,
            string FieldName)
        {
            return AtAddressEmitVariable.BuildConstantLoad(
                Builder,
                BuildStructGEP(
                    Builder,
                    Delegate,
                    FieldIndex,
                    FieldName + "_ptr"),
                FieldName);
        }

        /// <summary>
        /// Creates a sequence of instructions that loads a pointer to a
        /// delegate's context object.
        /// </summary>
        /// <param name="Builder">A block builder.</param>
        /// <param name="Delegate">A pointer to a delegate's data.</param>
        /// <returns>A pointer to the context object.</returns>
        public static LLVMValueRef BuildLoadContextObject(
            LLVMBuilderRef Builder,
            LLVMValueRef Delegate)
        {
            return BuildLoadFieldPointer(Builder, Delegate, 1, "context_obj");
        }

        /// <summary>
        /// Creates a sequence of instructions that loads a delegate's function pointer
        /// as an opaque pointer.
        /// </summary>
        /// <param name="Builder">A block builder.</param>
        /// <param name="Delegate">A pointer to a delegate's data.</param>
        /// <returns>An opaque pointer to the delegate's function.</returns>

        public static LLVMValueRef BuildLoadFunctionPointer(
            LLVMBuilderRef Builder,
            LLVMValueRef Delegate)
        {
            return BuildLoadFieldPointer(Builder, Delegate, 2, "function_ptr");
        }

        /// <summary>
        /// Creates a sequence of instructions that loads a delegate's has-context
        /// flag.
        /// </summary>
        /// <param name="Builder">A block builder.</param>
        /// <param name="Delegate">A pointer to a delegate's data.</param>
        /// <returns>A flag that tells if the delegate has a context value.</returns>
        public static LLVMValueRef BuildLoadHasContext(
            LLVMBuilderRef Builder,
            LLVMValueRef Delegate)
        {
            return BuildLoadFieldPointer(Builder, Delegate, 3, "has_context");
        }
    }
}