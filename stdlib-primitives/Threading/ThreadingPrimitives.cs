using pthread_t = ulong;
using pthread_attr_t = void;
// Actually, 'pthread_start_routine_t' has type 'void* (void*)'
using pthread_start_routine_t = void;

using ThreadStartRoutine = #builtin_delegate_type<void>;
using pthread_start_routine_delegate = #builtin_delegate_type<ThreadStartRoutine, void*>;

namespace System.Primitives.Threading
{
    /// <summary>
    /// A data structure that stores a thread's unique identifier.
    /// </summary>
    public struct ThreadId
    {
        internal pthread_t id;
    }

    public static class ThreadingPrimitives
    {
        /// <summary>
        /// Converts a delegate of a specified type to a function pointer that is callable from unmanaged code.
        /// </summary>
        /// <param name="value">The delegate to convert to a function pointer.</param>
        /// <returns>A function pointer.</returns>
        [#builtin_attribute(RuntimeImplementedAttribute)]
        private static pthread_start_routine_t* LoadDelegateFunctionPointerInternal(pthread_start_routine_delegate value);

        private static extern int pthread_create(
            pthread_t* thread, pthread_attr_t* attr,
            pthread_start_routine_t* start_routine, void* arg);

        private static extern int pthread_join(
            pthread_t thread, void* * retval);

        /// <summary>
        /// Starts a new thread with the given routine.
        /// </summary>
        /// <param name="startRoutine">The routine run by the thread.</param>
        /// <param name="threadId">The thread's identifier.</param>
        /// <returns><c>true</c> if the thread was created successfully; otherwise, <c>false</c>.</returns>
        public static bool CreateThread(
            ThreadStartRoutine startRoutine,
            out ThreadId threadId)
        {
            threadId = default(ThreadId);
            var startRoutinePtr = #builtin_ref_to_ptr(startRoutine);
            int errorCode = pthread_create(
                &threadId.id,
                (pthread_attr_t*)null,
                LoadDelegateFunctionPointerInternal(RunStartRoutine),
                startRoutinePtr);
            return errorCode == 0;
        }

        /// <summary>
        /// Waits for the thread with the given identifier to complete.
        /// </summary>
        /// <param name="threadId">A thread identifier.</param>
        /// <returns><c>true</c> if the thread was joined successfully; otherwise, <c>false</c>.</returns>
        public static bool JoinThread(ThreadId threadId)
        {
            int errorCode = pthread_join(threadId.id, (void* *)null);
            return errorCode == 0;
        }

        private static void* RunStartRoutine(
            ThreadStartRoutine startRoutine)
        {
            startRoutine();
            return (void*)null;
        }
    }
}