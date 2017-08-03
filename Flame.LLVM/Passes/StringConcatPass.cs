using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A node visitor that rewrites string concatenation as a function call.
    /// </summary>
    public sealed class StringConcatRewriter : NodeVisitorBase
    {
        /// <summary>
        /// Creates a string concatenation rewriter from the given environment.
        /// </summary>
        /// <param name="Environment">The environment that defines an equivalent type for System.String.</param>
        public StringConcatRewriter(IEnvironment Environment)
        {
            this.Environment = Environment;
            this.concatMethod = new Lazy<IMethod>(LookupConcatMethod);
        }

        /// <summary>
        /// Gets the environment this string concatenation rewriter.
        /// </summary>
        /// <returns>The environment.</returns>
        public IEnvironment Environment { get; private set; }

        private Lazy<IMethod> concatMethod;

        private IMethod LookupConcatMethod()
        {
            var strType = Environment.GetEquivalentType(PrimitiveTypes.String);
            var concatMethod = strType.GetMethod(
                new SimpleName("Concat"),
                true,
                PrimitiveTypes.String,
                new IType[] { PrimitiveTypes.String, PrimitiveTypes.String });

            if (concatMethod == null)
            {
                throw new NotImplementedException(
                    "System.String must define 'static string Concat(string, string)' " +
                    "for string concatenation to work.");
            }

            return concatMethod;
        }

        public override bool Matches(IStatement Value)
        {
            return false;
        }

        public override bool Matches(IExpression Value)
        {
            return Value is ConcatExpression;
        }

        private IExpression CreateConcatCalls(IReadOnlyList<IExpression> Operands)
        {
            if (Operands.Count == 0)
            {
                return new StringExpression("");
            }

            var result = Visit(Operands[0]);
            for (int i = 1; i < Operands.Count; i++)
            {
                result = new InvocationExpression(
                    concatMethod.Value,
                    null,
                    new IExpression[] { result, Visit(Operands[i]) });
            }
            return result;
        }

        protected override IExpression Transform(IExpression Expression)
        {
            // Rewrite string concatenation like
            //
            //     "abc" + "def"
            //
            // as
            //
            //     System.String.Concat("abc", "def")
            //
            var concatExpr = (ConcatExpression)Expression;
            if (concatExpr.Type == PrimitiveTypes.String)
            {
                return CreateConcatCalls(concatExpr.Operands);
            }
            else
            {
                return concatExpr.Accept(this);
            }
        }

        protected override IStatement Transform(IStatement Statement)
        {
            return Statement.Accept(this);
        }
    }

    /// <summary>
    /// A pass that lowers string concatenation as a function call.
    /// </summary>
    public sealed class StringConcatPass : IPass<BodyPassArgument, IStatement>
    {
        private StringConcatPass()
        { }

        /// <summary>
        /// An instance of the string concatenation lowering pass.
        /// </summary>
        public readonly static StringConcatPass Instance = new StringConcatPass();

        /// <summary>
        /// The name for the string concatenation lowering pass.
        /// </summary>
        public const string StringConcatPassName = "lower-string-concat";

        public IStatement Apply(BodyPassArgument Arg)
        {
            return new StringConcatRewriter(Arg.Environment).Visit(Arg.Body);
        }
    }
}