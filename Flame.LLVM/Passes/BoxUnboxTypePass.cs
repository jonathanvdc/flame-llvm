using System;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A node visitor that rewrites primitive box/unbox operations as box/unbox
    /// operations that rely on environment-equivalent types.
    /// </summary>
    public sealed class BoxUnboxTypeRewriter : NodeVisitorBase
    {
        /// <summary>
        /// Creates a box/unbox type rewriter from the given environment.
        /// </summary>
        /// <param name="Environment">The environment that defines the equivalent types to use.</param>
        public BoxUnboxTypeRewriter(IEnvironment Environment)
        {
            this.Environment = Environment;
        }

        /// <summary>
        /// Gets the environment this string concatenation rewriter.
        /// </summary>
        /// <returns>The environment.</returns>
        public IEnvironment Environment { get; private set; }

        private IType LookupEquivalentType(IType Type)
        {
            var equivType = Environment.GetEquivalentType(Type);

            if (equivType == null)
            {
                throw new NotImplementedException(
                    string.Format(
                        "An environment-equivalent type for '{0}' must be defined for " +
                        "boxing/unboxing to work.",
                        Type));
            }

            return equivType;
        }

        public override bool Matches(IStatement Value)
        {
            return false;
        }

        public override bool Matches(IExpression Value)
        {
            return Value is BoxExpression
                || Value is UnboxReferenceExpression
                || Value is UnboxValueExpression
                || Value is IsExpression;
        }

        protected override IExpression Transform(IExpression Expression)
        {
            if (Expression is BoxExpression)
            {
                var boxExpr = (BoxExpression)Expression;
                var valType = boxExpr.Value.Type;
                if (!PrimitiveTypes.GetIsPrimitive(valType))
                {
                    return boxExpr.Accept(this);
                }
                else
                {
                    return new BoxExpression(
                        new ReinterpretCastExpression(
                            Visit(boxExpr.Value),
                            LookupEquivalentType(valType)));
                }
            }
            else if (Expression is IsExpression)
            {
                var isExpr = (IsExpression)Expression;
                if (!PrimitiveTypes.GetIsPrimitive(isExpr.TestType))
                {
                    return isExpr.Accept(this);
                }
                else
                {
                    return new IsExpression(
                        Visit(isExpr.Target),
                        LookupEquivalentType(isExpr.TestType));
                }
            }
            else if (Expression is UnboxReferenceExpression)
            {
                var unboxRefExpr = (UnboxReferenceExpression)Expression;
                var ptrType = unboxRefExpr.TargetType.AsPointerType();
                var elemType = ptrType.ElementType;
                if (!PrimitiveTypes.GetIsPrimitive(elemType))
                {
                    return unboxRefExpr.Accept(this);
                }
                else
                {
                    return new ReinterpretCastExpression(
                        new UnboxReferenceExpression(
                            Visit(unboxRefExpr.Value),
                            LookupEquivalentType(elemType).MakePointerType(ptrType.PointerKind)),
                        unboxRefExpr.TargetType);
                }
            }
            else
            {
                var unboxExpr = (UnboxValueExpression)Expression;
                if (!PrimitiveTypes.GetIsPrimitive(unboxExpr.TargetType))
                {
                    return unboxExpr.Accept(this);
                }
                else
                {
                    return new ReinterpretCastExpression(
                        new UnboxValueExpression(
                            Visit(unboxExpr.Value),
                            LookupEquivalentType(unboxExpr.TargetType)),
                        unboxExpr.TargetType);
                }
            }
        }

        protected override IStatement Transform(IStatement Statement)
        {
            return Statement.Accept(this);
        }
    }

    /// <summary>
    /// A pass that rewrites the types of box/unbox operations as
    /// environment-equivalent types.
    /// </summary>
    public sealed class BoxUnboxTypePass : IPass<BodyPassArgument, IStatement>
    {
        private BoxUnboxTypePass()
        { }

        /// <summary>
        /// An instance of the box/unbox type lowering pass.
        /// </summary>
        public readonly static BoxUnboxTypePass Instance = new BoxUnboxTypePass();

        /// <summary>
        /// The name for the box/unbox type lowering pass.
        /// </summary>
        public const string BoxUnboxTypePassName = "lower-box-unbox-types";

        public IStatement Apply(BodyPassArgument Arg)
        {
            return new BoxUnboxTypeRewriter(Arg.Environment).Visit(Arg.Body);
        }
    }
}