using System;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;

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

        /// <summary>
        /// Registers a finalizer for the given garbage-collected object.
        /// </summary>
        /// <param name="Pointer">A pointer to a garbage-collected object.</param>
        /// <param name="Finalizer">The finalizer method to register.</param>
        /// <returns>A statement that registers a finalizer.</returns>
        public abstract IStatement RegisterFinalizer(IExpression Pointer, IMethod Finalizer);
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
        /// A method that allocates data. It must have signature `static void*(ulong)`.
        /// </param>
        /// <param name="RegisterFinalizerMethod">
        /// A method that registers a finalizer for an object. 
        /// </param>
        public ExternalGCDescription(Lazy<IMethod> AllocateMethod, Lazy<IMethod> RegisterFinalizerMethod)
        {
            this.allocMethod = AllocateMethod;
            this.registerFinalizerMethod = RegisterFinalizerMethod;
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
            this.registerFinalizerMethod = methodFinder.RegisterFinalizerMethod;
        }

        private Lazy<IMethod> allocMethod;
        private Lazy<IMethod> registerFinalizerMethod;

        /// <summary>
        /// Gets the method that allocates data. It must have signature `static void*(ulong)`.
        /// </summary>
        /// <returns>The allocation method.</returns>
        public IMethod AllocateMethod => allocMethod.Value;

        /// <summary>
        /// Gets the method that registers finalizers. It must have signature
        /// `static void(void*, void(void*))'
        /// </summary>
        public IMethod RegisterFinalizerMethod => registerFinalizerMethod.Value;

        /// <inheritdoc/>
        public override IExpression Allocate(IExpression Size)
        {
            return new InvocationExpression(AllocateMethod, null, new IExpression[] { Size });
        }

        /// <inheritdoc/>
        public override IStatement RegisterFinalizer(IExpression Pointer, IMethod Finalizer)
        {
            return new ExpressionStatement(
                new InvocationExpression(
                    RegisterFinalizerMethod,
                    null,
                    new IExpression[]
                    {
                        Pointer,
                        new GetMethodExpression(Finalizer, null)
                    }));
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
            this.RegisterFinalizerMethod = new Lazy<IMethod>(GetRegisterFinalizerMethod);
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
        /// Gets the allocation method for this method finder.
        /// </summary>
        /// <returns>The allocation method.</returns>
        public Lazy<IMethod> AllocateMethod { get; private set; }

        /// <summary>
        /// Gets the finalizer registration method for this method founder.
        /// </summary>
        /// <returns>The finalizer registration method.</returns>
        public Lazy<IMethod> RegisterFinalizerMethod { get; private set; }

        // The GC type is called "__compiler_rt.GC".
        private static readonly QualifiedName GCTypeName =
            new SimpleName("GC")
            .Qualify(
                new SimpleName("__compiler_rt")
                .Qualify());

        // The allocation function's unqualified name is "Allocate".
        private const string AllocateMethodName = "Allocate";

        private const string RegisterFinalizerMethodName = "RegisterFinalizer";

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
                        "cannot find runtime method 'static void* " +
                        new SimpleName(AllocateMethodName).Qualify(GCTypeName).ToString() +
                        "(ulong)', which must be present for garbage collection to work."));
            }
            return method;
        }

        private IMethod GetRegisterFinalizerMethod()
        {
            var gcType = GCType.Value;
            if (gcType == null)
                return null;

            var voidPtr = PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer);

            var callbackSignature = new DescribedMethod("", null, PrimitiveTypes.Void, true);
            callbackSignature.AddParameter(new DescribedParameter("ptr", voidPtr));

            var method = gcType.GetMethod(
                new SimpleName(RegisterFinalizerMethodName),
                true,
                PrimitiveTypes.Void,
                new IType[]
                {
                    voidPtr,
                    MethodType.Create(callbackSignature)
                });

            if (method == null)
            {
                Log.LogError(
                    new LogEntry(
                        "missing runtime method",
                        "cannot find runtime method 'static void " +
                        new SimpleName(RegisterFinalizerMethodName).Qualify(GCTypeName).ToString() +
                        "(void*, void(void*))', which must be present for finalizers to work."));
            }
            return method;
        }
    }
}