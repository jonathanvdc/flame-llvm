using System.Primitives.InteropServices;

using pthread_mutex_t = void;
using pthread_mutexattr_t = void;

namespace System.Primitives.Threading
{
    /// <summary>
    /// A handle to a mutex.
    /// </summary>
    public struct MutexHandle
    {
        /// <summary>
        /// Checks if this is a null handle, i.e., it is a signaling value that
        /// does not refer to an actual mutex.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this handle is a null handle; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNull =>
            MutexPointer == null
            && AttributePointer == null;

        internal pthread_mutex_t* MutexPointer { get; private set; }
        internal pthread_mutexattr_t* AttributePointer { get; private set; }

        private static extern uint pthread_get_mutex_size();

        private static extern uint pthread_get_mutexattr_size();

        internal static MutexHandle Allocate()
        {
            var result = default(MutexHandle);
            result.AttributePointer = Memory.AllocHGlobal(pthread_get_mutexattr_size());
            result.MutexPointer = Memory.AllocHGlobal(pthread_get_mutex_size());
            return result;
        }

        internal void Free()
        {
            Memory.FreeHGlobal(MutexPointer);
            Memory.FreeHGlobal(AttributePointer);
            MutexPointer = null;
            AttributePointer = null;
        }
    }

    public static class MutexPrimitives
    {
        private static extern int pthread_get_mutex_errorcheck_type();

        private static extern int pthread_mutexattr_init(
            pthread_mutexattr_t* attr);

        private static extern int pthread_mutexattr_settype(
            pthread_mutexattr_t* attr, int type);

        private static extern int pthread_mutexattr_destroy(
            pthread_mutexattr_t* attr);

        private static extern int pthread_mutex_init(
            pthread_mutex_t* mutex, pthread_mutexattr_t* attr);

        private static extern int pthread_mutex_destroy(
            pthread_mutex_t* mutex);

        private static extern int pthread_mutex_lock(
            pthread_mutex_t* mutex);

        private static extern int pthread_mutex_trylock(
            pthread_mutex_t* mutex);

        private static extern int pthread_mutex_unlock(
            pthread_mutex_t* mutex);

        /// <summary>
        /// Creates a mutex.
        /// </summary>
        /// <param name="mutex">A handle in which a mutex will be stored.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode CreateMutex(out MutexHandle mutex)
        {
            mutex = MutexHandle.Allocate();

            var result = (ThreadResultCode)pthread_mutexattr_init(mutex.AttributePointer);
            if (result != ThreadResultCode.Success)
            {
                mutex.Free();
                return result;
            }

            result = (ThreadResultCode)pthread_mutexattr_settype(
                mutex.AttributePointer, pthread_get_mutex_errorcheck_type());

            if (result != ThreadResultCode.Success)
            {
                pthread_mutexattr_destroy(mutex.AttributePointer);
                mutex.Free();
                return result;
            }

            result = (ThreadResultCode)pthread_mutex_init(
                mutex.MutexPointer, mutex.AttributePointer);

            if (result != ThreadResultCode.Success)
            {
                pthread_mutexattr_destroy(mutex.AttributePointer);
                mutex.Free();
                return result;
            }
            return result;
        }

        /// <summary>
        /// Releases resources acquired by a mutex.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode DisposeMutex(MutexHandle mutex)
        {
            if (mutex.IsNull)
            {
                // Nothing to do here I guess.
                return ThreadResultCode.Success;
            }

            var mutexResult = (ThreadResultCode)pthread_mutex_destroy(mutex.MutexPointer);
            var attrResult = (ThreadResultCode)pthread_mutexattr_destroy(mutex.AttributePointer);
            mutex.Free();
            return mutexResult == ThreadResultCode.Success
                ? attrResult
                : mutexResult;
        }

        /// <summary>
        /// Acquires a lock on a mutex.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode LockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_lock(mutex.MutexPointer);
        }

        /// <summary>
        /// Acquires a lock on a mutex if one has not been acquired already.
        /// If a lock has been acquired, then this function fails immediately.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode TryLockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_trylock(mutex.MutexPointer);
        }

        /// <summary>
        /// Releases a lock on a mutex.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode UnlockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_unlock(mutex.MutexPointer);
        }
    }
}