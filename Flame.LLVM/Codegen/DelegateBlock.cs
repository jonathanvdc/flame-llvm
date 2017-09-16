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
        /// Gets the data layout of a method type.
        /// </summary>
        public static LLVMTypeRef GetMethodTypeLayout(IMethod Method, LLVMModuleBuilder Module) =>
            StructType(
                new LLVMTypeRef[]
                {
                    PointerType(Int8Type(), 0),
                    PointerType(Int8Type(), 0),
                    PointerType(Module.DeclarePrototype(Method), 0)
                },
                false);

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var targetAndBlock = InvocationBlock.EmitTarget(BasicBlock, Target);
            BasicBlock = targetAndBlock.BasicBlock;

            var calleeAndBlock = InvocationBlock.EmitCallee(BasicBlock, targetAndBlock.Value, Callee, Op);
            BasicBlock = calleeAndBlock.BasicBlock;

            var layout = GetMethodTypeLayout(Callee, BasicBlock.FunctionBody.Module);

            var methodTypeAllocBlock = (CodeBlock)codeGen.Allocate(
                new StaticCastExpression(
                    LLVMCodeGenerator.ToExpression(
                        new ConstantBlock(codeGen, PrimitiveTypes.UInt64, SizeOf(layout))),
                    PrimitiveTypes.UInt64),
                Type).Emit(codeGen);

            var methodTypeAllocCodegen = methodTypeAllocBlock.Emit(BasicBlock);
            BasicBlock = methodTypeAllocCodegen.BasicBlock;

            BuildStore(
                BasicBlock.Builder,
                TypeVTableBlock.BuildTypeVTable(BasicBlock, Type),
                BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 0, "vtable_ptr"));

            if (targetAndBlock.Value.Pointer != IntPtr.Zero)
            {
                BuildStore(
                    BasicBlock.Builder,
                    targetAndBlock.Value,
                    BuildBitCast(
                        BasicBlock.Builder,
                        BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 1, "context_ptr"),
                        PointerType(targetAndBlock.Value.TypeOf(), 0),
                        "context_obj"));
            }

            BuildStore(
                BasicBlock.Builder,
                calleeAndBlock.Value,
                BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 2, "func_ptr"));

            return new BlockCodegen(BasicBlock, methodTypeAllocCodegen.Value);
        }
    }
}