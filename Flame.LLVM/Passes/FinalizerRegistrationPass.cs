using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;
using Flame.Compiler.Variables;
using Flame.Compiler.Visitors;

namespace Flame.LLVM.Passes
{
    /// <summary>
    /// A pass that registers finalizers.
    /// </summary>
    public sealed class FinalizerRegistrationVisitor : NodeVisitorBase
    {
        /// <summary>
        /// Creates a finalizer registration visitor from a garbage collector description.
        /// </summary>
        /// <param name="GarbageCollector">A GC description.</param>
        public FinalizerRegistrationVisitor(GCDescription GarbageCollector)
        {
            this.GarbageCollector = GarbageCollector;
        }

        /// <summary>
        /// Gets the garbage collector description used by this visitor.
        /// </summary>
        /// <returns>The garbage collector description.</returns>
        public GCDescription GarbageCollector { get; private set; }

        /// <inheritdoc/>
        public override bool Matches(IStatement Value)
        {
            return false;
        }

        /// <inheritdoc/>
        public override bool Matches(IExpression Value)
        {
            return Value is NewObjectExpression;
        }

        /// <inheritdoc/>
        protected override IExpression Transform(IExpression Expression)
        {
            if (Expression is NewObjectExpression)
            {
                var expr = (NewObjectExpression)Expression;
                var visitedExpr = expr.Accept(this);
                var constructedType = expr.Type;
                if (!constructedType.GetIsReferenceType())
                {
                    return visitedExpr;
                }

                // TODO: a type's finalizer is looked up every time an object is constructed.
                // That's a pretty expensive operation and we could speed it up greatly
                // by introducing a cache.
                var rootAncestor = GetRootAncestor(constructedType);
                var rootFinalizer = GetFinalizer(rootAncestor);

                if (rootFinalizer == null)
                {
                    return visitedExpr;
                }

                var finalizer = rootFinalizer.GetImplementation(constructedType) ?? rootFinalizer;

                if (finalizer.HasAttribute(
                    PrimitiveAttributes.Instance.NopAttribute.AttributeType))
                {
                    return visitedExpr;
                }

                var tmp = new SSAVariable(constructedType);

                return new InitializedExpression(
                    new BlockStatement(
                        new IStatement[]
                        {
                            tmp.CreateSetStatement(visitedExpr),
                            GarbageCollector.RegisterFinalizer(
                                new ReinterpretCastExpression(
                                    tmp.CreateGetExpression(),
                                    PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer)),
                                finalizer)
                        }),
                    tmp.CreateGetExpression());
            }
            else
            {
                return Expression.Accept(this);
            }
        }

        /// <inheritdoc/>
        protected override IStatement Transform(IStatement Statement)
        {
            return Statement.Accept(this);
        }

        private static IMethod GetFinalizer(IType Type)
        {
            return Type.GetMethod(
                new SimpleName("Finalize"),
                false,
                PrimitiveTypes.Void,
                new IType[] { });
        }

        private static IType GetRootAncestor(IType Type)
        {
            var parent = Type.GetParent();
            if (parent == null)
                return Type;
            else
                return GetRootAncestor(parent);
        }
    }

    /// <summary>
    /// A pass that registers finalizers.
    /// </summary>
    public sealed class FinalizerRegistrationPass : IPass<IStatement, IStatement>
    {
        /// <summary>
        /// Creates a finalizer registration pass from a garbage collector description.
        /// </summary>
        /// <param name="GarbageCollector">A GC description.</param>
        public FinalizerRegistrationPass(GCDescription GarbageCollector)
        {
            this.GarbageCollector = GarbageCollector;
        }

        /// <summary>
        /// Gets the garbage collector description used by this pass.
        /// </summary>
        /// <returns>The garbage collector description.</returns>
        public GCDescription GarbageCollector { get; private set; }

        /// <summary>
        /// The name for the finalizer registration pass.
        /// </summary>
        public const string FinalizerRegistrationPassName = "register-finalizers";

        /// <inheritdoc/>
        public IStatement Apply(IStatement Value)
        {
            return new FinalizerRegistrationVisitor(GarbageCollector).Visit(Value);
        }
    }
}