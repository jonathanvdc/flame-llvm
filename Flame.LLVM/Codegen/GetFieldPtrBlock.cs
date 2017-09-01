using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that produces a pointer to a field.
    /// </summary>
    public sealed class GetFieldPtrBlock : CodeBlock
    {
        /// <summary>
        /// Creates a field pointer block from the given target pointer and a field.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Target">The object in which a field is addressed.</param>
        /// <param name="Field">The field to address.</param>
        public GetFieldPtrBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Target,
            LLVMField Field)
        {
            this.codeGen = CodeGenerator;
            this.Target = Target;
            this.Field = Field;
        }

        /// <summary>
        /// Creates a field pointer block from the given static field.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Field">The field to address.</param>
        public GetFieldPtrBlock(
            ICodeGenerator CodeGenerator,
            LLVMField Field)
        {
            this.codeGen = CodeGenerator;
            this.Field = Field;
        }

        /// <summary>
        /// Gets a block that produces a pointer to the object in which a field is addressed.
        /// </summary>
        /// <returns>The object in which a field is addressed.</returns>
        public CodeBlock Target { get; private set; }

        /// <summary>
        /// Gets the field to load a pointer to.
        /// </summary>
        /// <returns>The field to address.</returns>
        public LLVMField Field { get; private set; }

        /// <summary>
        /// Tests if a static field is accessed by this block.
        /// </summary>
        public bool AccessesStaticField => Target == null;

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => Field.FieldType.MakePointerType(PointerKind.ReferencePointer);

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            if (AccessesStaticField)
            {
                // Run the static constructors of a type before accessing
                // any of its fields.
                var declType = Field.DeclaringType as LLVMType;
                if (declType != null)
                {
                    if (!CanElideStaticConstructorCall(codeGen.Method, declType))
                    {
                        BasicBlock = BasicBlock.FunctionBody.Module.EmitRunStaticConstructors(BasicBlock, declType);
                    }
                }
                return new BlockCodegen(
                    BasicBlock,
                    BasicBlock.FunctionBody.Module.DeclareGlobal(Field));
            }

            var targetResult = Target.Emit(BasicBlock);
            BasicBlock = targetResult.BasicBlock;
            if (Field.IsSingleValueField)
            {
                return new BlockCodegen(BasicBlock, targetResult.Value);
            }
            else
            {
                return new BlockCodegen(
                    BasicBlock,
                    BuildStructGEP(
                        BasicBlock.Builder,
                        targetResult.Value,
                        (uint)Field.FieldIndex,
                        "field_ptr_tmp"));
            }
        }

        private static bool CanElideStaticConstructorCall(
            IMethod CurrentMethod,
            IType ConstructedType)
        {
            // Static constructors cannot ever call themselves because they only get
            // called once.
            return CurrentMethod.IsConstructor
                && CurrentMethod.IsStatic
                && object.Equals(CurrentMethod.DeclaringType, ConstructedType);
        }
    }
}