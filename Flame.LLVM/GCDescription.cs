using System;
using Flame.Compiler;
using Flame.Compiler.Expressions;

namespace Flame.LLVM
{
    /// <summary>
    /// Describes how the back-end interfaces with the garbage collector.
    /// </summary>
    public abstract class GCDescription
    {
        /// <summary>
        /// Creates an expression that allocates an object of the given size.
        /// </summary>
        /// <param name="Size">The size of the object to allocate, in bytes.</param>
        /// <returns>An expression that allocates an object and returns a pointer to it.</returns>
        public abstract IExpression Allocate(IExpression Size);
    }

    /// <summary>
    /// The default GC description, which relies on user-defined methods to work.
    /// </summary>
    public sealed class ExternalGCDescription : GCDescription
    {
        /// <summary>
        /// Creates a GC description from the given allocation method.
        /// </summary>
        /// <param name="AllocateMethod">
        /// The method that allocates data. It must have signature `static void*(ulong)`.
        /// </param>
        public ExternalGCDescription(Lazy<IMethod> AllocateMethod)
        {
            this.allocMethod = AllocateMethod;
        }

        /// <summary>
        /// Creates a GC description from the given binder and log.
        /// </summary>
        /// <param name="Binder">The binder to find types with.</param>
        /// <param name="Log">The log to use when an error is to be reported.</param>
        public ExternalGCDescription(IBinder Binder, ICompilerLog Log)
        {
            var methodFinder = new ExternalGCMethodFinder(Binder, Log);
            this.allocMethod = methodFinder.AllocateMethod;
        }

        private Lazy<IMethod> allocMethod;

        /// <summary>
        /// Gets the method that allocates data. It must have signature `static void*(ulong)`.
        /// </summary>
        /// <returns>The allocation method.</returns>
        public IMethod AllocateMethod => allocMethod.Value;

        /// <inheritdoc/>
        public override IExpression Allocate(IExpression Size)
        {
            return new InvocationExpression(AllocateMethod, null, new IExpression[] { Size });
        }
    }

    /// <summary>
    /// A helper class that finds user-defined GC methods.
    /// </summary>
    internal sealed class ExternalGCMethodFinder
    {
        /// <summary>
        /// Creates a GC method finder from the given binder and log.
        /// </summary>
        /// <param name="Binder">The binder to find types with.</param>
        /// <param name="Log">The log to use when an error is to be reported.</param>
        public ExternalGCMethodFinder(IBinder Binder, ICompilerLog Log)
        {
            this.Binder = Binder;
            this.Log = Log;
            this.GCType = new Lazy<IType>(GetGCType);
            this.AllocateMethod = new Lazy<IMethod>(GetAllocateMethod);
        }

        /// <summary>
        /// Gets the binder to use.
        /// </summary>
        /// <returns>The binder.</returns>
        public IBinder Binder { get; private set; }

        /// <summary>
        /// Gets the compiler log for this GC method finder.
        /// </summary>
        /// <returns>The compiler log.</returns>
        public ICompilerLog Log { get; private set; }

        /// <summary>
        /// Gets the runtime GC implementation type for this method finder.
        /// </summary>
        /// <returns>The runtime GC type.</returns>
        public Lazy<IType> GCType { get; private set; }

        /// <summary>
        /// Gets the allocation method this method finder.
        /// </summary>
        /// <returns>The allocation method.</returns>
        public Lazy<IMethod> AllocateMethod { get; private set; }

        // The GC type is called "__compiler_rt.GC".
        private static readonly QualifiedName GCTypeName =
            new SimpleName("GC")
            .Qualify(
                new SimpleName("__compiler_rt")
                .Qualify());

        // The allocation function's unqualified name is "Allocate".
        private const string AllocateMethodName = "Allocate";

        private IType GetGCType()
        {
            var gcType = Binder.BindType(GCTypeName);
            if (gcType == null)
            {
                Log.LogError(
                    new LogEntry(
                        "missing runtime type",
                        "cannot find runtime type '" + GCTypeName.ToString() +
                        "', which must be present for garbage collection to work."));
            }
            return gcType;
        }

        private IMethod GetAllocateMethod()
        {
            var gcType = GCType.Value;
            if (gcType == null)
                return null;

            var method = gcType.GetMethod(
                new SimpleName(AllocateMethodName),
                true,
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                new IType[] { PrimitiveTypes.UInt64 });

            if (method == null)
            {
                Log.LogError(
                    new LogEntry(
                        "missing runtime method",
                        "cannot find runtime method 'void* " +
                        new SimpleName(AllocateMethodName).Qualify(GCTypeName).ToString() +
                        "(ulong)', which must be present for garbage collection to work."));
            }
            return method;
        }
    }
}