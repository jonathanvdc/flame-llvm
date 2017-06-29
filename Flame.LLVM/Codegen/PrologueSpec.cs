using System.Collections.Generic;
using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// Specifies a prologue.
    /// </summary>
    public sealed class PrologueSpec
    {
        public PrologueSpec()
        {
            this.taggedInstructions = new List<KeyValuePair<UniqueTag, CodeBlock>>();
        }

        private List<KeyValuePair<UniqueTag, CodeBlock>> taggedInstructions;

        /// <summary>
        /// Adds an instruction to this prologue.
        /// </summary>
        /// <param name="Instruction">The instruction to add to the prologue.</param>
        /// <returns>A unique tag that can be used to retrieve the instruction's codegen value.</returns>
        public UniqueTag AddInstruction(CodeBlock Instruction)
        {
            var tag = new UniqueTag();
            taggedInstructions.Add(new KeyValuePair<UniqueTag, CodeBlock>(tag, Instruction));
            return tag;
        }

        /// <summary>
        /// Emits this prologue spec to the given basic block.
        /// </summary>
        /// <param name="BasicBlock">The basic block to populate.</param>
        /// <returns>The next basic block.</returns>
        public BasicBlockBuilder Emit(BasicBlockBuilder BasicBlock)
        {
            for (int i = 0; i < taggedInstructions.Count; i++)
            {
                var codegen = taggedInstructions[i].Value.Emit(BasicBlock);
                BasicBlock = codegen.BasicBlock;
                BasicBlock.FunctionBody.TagValue(taggedInstructions[i].Key, codegen.Value);
            }
            return BasicBlock;
        }
    }
}