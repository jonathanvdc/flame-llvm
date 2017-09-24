using GC_obj = void;
using GC_finalization_proc = void;
using GC_size_t = ulong;
using GC_finalization_delegate = #builtin_delegate_type<GC_obj*, void>;

namespace __compiler_rt
{
    using GC_run_finalizer_delegate = #builtin_delegate_type<GC_obj*, GC_finalization_delegate, void>;

    /// <summary>
    /// Garbage collection functionality that can be used by the compiler.
    /// </summary>
    public static unsafe class GC
    {
        [#builtin_attribute(NoAliasAttribute)]
        [#builtin_attribute(NoThrowAttribute)]
        private static extern GC_obj* GC_malloc(GC_size_t size);

        [#builtin_attribute(NoThrowAttribute)]
        private static extern void GC_register_finalizer(
            GC_obj* obj,
            GC_finalization_proc* finalizer,
            GC_obj* finalizerContext,
            out GC_finalization_proc* oldFinalizer,
            out GC_obj* oldFinalizerContext);

        /// <summary>
        /// Converts a delegate of a specified type to a function pointer that is callable from unmanaged code.
        /// </summary>
        /// <param name="value">The delegate to convert to a function pointer.</param>
        /// <returns>A function pointer.</returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static GC_finalization_proc* LoadDelegateFunctionPointerInternal(GC_run_finalizer_delegate value);

        /// <summary>
        /// Allocates a region of storage that is a number of bytes in size.
        /// The storage is zero-initialized and a pointer to it is returned.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        /// <returns>A pointer to a region of storage.</returns>
        [#builtin_attribute(NoAliasAttribute)]
        [#builtin_attribute(NoThrowAttribute)]
        public static GC_obj* Allocate(GC_size_t size)
        {
            return GC_malloc(size);
        }

        /// <summary>
        /// Registers a finalizer to run when an object is garbage-collected.
        /// </summary>
        /// <param name="obj">A pointer to a garbage-collected object.</param>
        /// <param name="finalizer">
        /// A finalizer delegate to run when <c>obj</c> is garbage-collected.
        /// </param>
        [#builtin_hidden]
        [#builtin_attribute(NoThrowAttribute)]
        public static void RegisterFinalizer(
            GC_obj* obj,
            GC_finalization_delegate finalizer)
        {
            GC_finalization_proc* oldFinalizer;
            GC_obj* oldFinalizerContext;
            GC_register_finalizer(
                obj,
                LoadDelegateFunctionPointerInternal(RunFinalizer),
                #builtin_ref_to_ptr(finalizer),
                out oldFinalizer,
                out oldFinalizerContext);
        }

        private static void RunFinalizer(GC_obj* obj, GC_finalization_delegate finalizer)
        {
            finalizer(obj);
        }
    }
}
