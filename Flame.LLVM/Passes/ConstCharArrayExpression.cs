using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.LLVM.Codegen;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// An expression that produces a constant character array.
    /// </summary>
    public sealed class ConstCharArrayExpression : IExpression
    {
        /// <summary>
        /// Creates a new constant character array expression from the
        /// given string of characters.
        /// </summary>
        /// <param name="Data">The string of characters to create an expression from.</param>
        public ConstCharArrayExpression(string Data)
        {
            this.Data = Data;
        }

        /// <summary>
        /// Gets the backing data for this constant character array.
        /// </summary>
        public string Data { get; private set; }

        public IType Type => PrimitiveTypes.Char.MakeArrayType(1);

        public bool IsConstantNode => true;

        public IExpression Accept(INodeVisitor Visitor)
        {
            return this;
        }

        public ICodeBlock Emit(ICodeGenerator CodeGenerator)
        {
            if (CodeGenerator is LLVMCodeGenerator)
            {
                return new ConstCharArrayBlock((LLVMCodeGenerator)CodeGenerator, Data);
            }
            else
            {
                return CreateCharacterArrayExpression(Data).Emit(CodeGenerator);
            }
        }

        private static IExpression CreateCharacterArrayExpression(string Value)
        {
            return new InitializedArrayExpression(
                PrimitiveTypes.Char,
                Enumerable.Select<char, IExpression>(Value, CreateCharExpression).ToArray<IExpression>());
        }

        private static IExpression CreateCharExpression(char Value)
        {
            return new CharExpression(Value);
        }

        public IBoundObject Evaluate()
        {
            return null;
        }

        public IExpression Optimize()
        {
            return this;
        }
    }
}