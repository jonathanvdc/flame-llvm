using Flame.Compiler;
using Flame.Compiler.Expressions;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block that converts a delegate of one type to a delegate of another.
    /// </summary>
    public sealed class DelegateCastBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that converts a delegate of one type to a delegate of another.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Delegate">The delegate to convert.</param>
        /// <param name="Type">The type to which the delegate is converted.</param>
        public DelegateCastBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock Delegate,
            IType Type)
        {
            this.codeGen = CodeGenerator;
            this.Delegate = Delegate;
            this.targetType = Type;
        }

        /// <summary>
        /// Gets a code block that produces the delegate to convert.
        /// </summary>
        /// <returns>The target on which the callee is invoked.</returns>
        public CodeBlock Delegate { get; private set; }

        private IType targetType;

        /// <inheritdoc/>
        private LLVMCodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => targetType;

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var delegateAndBlock = Delegate.Emit(BasicBlock);
            BasicBlock = delegateAndBlock.BasicBlock;

            var methodTypeAllocBlock = (CodeBlock)codeGen.Allocate(
                new StaticCastExpression(
                    LLVMCodeGenerator.ToExpression(
                        new ConstantBlock(codeGen, PrimitiveTypes.UInt64, SizeOf(DelegateBlock.MethodTypeLayout))),
                    PrimitiveTypes.UInt64),
                Type).Emit(codeGen);

            var methodTypeAllocCodegen = methodTypeAllocBlock.Emit(BasicBlock);
            BasicBlock = methodTypeAllocCodegen.BasicBlock;

            BuildStore(
                BasicBlock.Builder,
                AtAddressEmitVariable.BuildConstantLoad(
                    BasicBlock.Builder,
                    BuildBitCast(
                        BasicBlock.Builder,
                        delegateAndBlock.Value,
                        PointerType(DelegateBlock.MethodTypeLayout, 0),
                        "delegate_ptr"),
                    "delegate_data"),
                methodTypeAllocCodegen.Value);

            BuildStore(
                BasicBlock.Builder,
                TypeVTableBlock.BuildTypeVTable(BasicBlock, Type),
                BuildStructGEP(BasicBlock.Builder, methodTypeAllocCodegen.Value, 0, "vtable_ptr_ptr"));

            return new BlockCodegen(BasicBlock, methodTypeAllocCodegen.Value);
        }
    }
}