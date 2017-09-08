using System;
using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A node visitor that rewrites string literals as function calls.
    /// </summary>
    public sealed class StringLiteralRewriter : NodeVisitorBase
    {
        /// <summary>
        /// Creates a string literal rewriter from the given environment.
        /// </summary>
        /// <param name="Environment">The environment that defines an equivalent type for System.String.</param>
        public StringLiteralRewriter(IEnvironment Environment)
        {
            this.Environment = Environment;
            this.fromConstCharArrayMethod = new Lazy<IMethod>(LookupFromCharArrayMethod);
        }

        /// <summary>
        /// Gets the environment this string literal rewriter.
        /// </summary>
        /// <returns>The environment.</returns>
        public IEnvironment Environment { get; private set; }

        private Lazy<IMethod> fromConstCharArrayMethod;

        private IMethod LookupFromCharArrayMethod()
        {
            var strType = Environment.GetEquivalentType(PrimitiveTypes.String);
            var fromConstArrMethod = strType.GetMethod(
                new SimpleName("FromConstCharArray"),
                true,
                PrimitiveTypes.String,
                new IType[] { PrimitiveTypes.Char.MakeArrayType(1) });

            if (fromConstArrMethod == null)
            {
                throw new NotImplementedException(
                    "System.String must define 'static string FromConstCharArray(char[])' " +
                    "for string literals to work.");
            }

            return fromConstArrMethod;
        }

        public override bool Matches(IStatement Value)
        {
            return false;
        }

        public override bool Matches(IExpression Value)
        {
            return Value is StringExpression;
        }

        protected override IExpression Transform(IExpression Expression)
        {
            // Rewrite string literals like
            //
            //     "abc"
            //
            // as
            //
            //     System.String.FromConstCharArray(new char[] { 'a', 'b', 'c' })
            //
            var strExpr = (StringExpression)Expression;
            return new InvocationExpression(
                fromConstCharArrayMethod.Value,
                null,
                new IExpression[] { new ConstCharArrayExpression(strExpr.Value) });
        }

        protected override IStatement Transform(IStatement Statement)
        {
            return Statement.Accept(this);
        }
    }

    /// <summary>
    /// A pass that lowers string literals as function calls.
    /// </summary>
    public sealed class StringLiteralPass : IPass<BodyPassArgument, IStatement>
    {
        private StringLiteralPass()
        { }

        /// <summary>
        /// An instance of the string literal lowering pass.
        /// </summary>
        public readonly static StringLiteralPass Instance = new StringLiteralPass();

        /// <summary>
        /// The name for the string literal lowering pass.
        /// </summary>
        public const string StringLiteralPassName = "lower-string-literals";

        public IStatement Apply(BodyPassArgument Arg)
        {
            return new StringLiteralRewriter(Arg.Environment).Visit(Arg.Body);
        }
    }
}