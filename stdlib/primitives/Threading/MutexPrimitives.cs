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
        public bool IsNull => ptr == (pthread_mutex_t*)null;

        internal pthread_mutex_t* ptr;
    }

    public static class MutexPrimitives
    {
        private static extern uint pthread_get_mutex_size();

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
            var mutexPtr = Memory.AllocHGlobal(pthread_get_mutex_size());

            var result = (ThreadResultCode)pthread_mutex_init(
                mutexPtr, (pthread_mutexattr_t*)null);

            mutex = default(MutexHandle);
            if (result == ThreadResultCode.Success)
            {
                mutex.ptr = mutexPtr;
            }
            else
            {
                Memory.FreeHGlobal(mutexPtr);
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

            var result = (ThreadResultCode)pthread_mutex_destroy(mutex.ptr);
            Memory.FreeHGlobal(mutex.ptr);
            return result;
        }

        /// <summary>
        /// Acquires a lock on a mutex.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode LockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_lock(mutex.ptr);
        }

        /// <summary>
        /// Acquires a lock on a mutex if one has not been acquired already.
        /// If a lock has been acquired, then this function fails immediately.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode TryLockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_trylock(mutex.ptr);
        }

        /// <summary>
        /// Releases a lock on a mutex.
        /// </summary>
        /// <param name="mutex">A handle to a mutex.</param>
        /// <returns>A result code.</returns>
        public static ThreadResultCode UnlockMutex(MutexHandle mutex)
        {
            return (ThreadResultCode)pthread_mutex_unlock(mutex.ptr);
        }
    }
}