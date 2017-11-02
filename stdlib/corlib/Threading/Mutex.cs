using System.Primitives.Threading;

namespace System.Threading
{
    public sealed class Mutex : WaitHandle
    {
        public Mutex()
        {
            var code = MutexPrimitives.CreateMutex(out this.handle);
            if (code != ThreadResultCode.Success)
            {
                throw new Exception("Mutex creation failed with error code '" + code + "'.");
            }
        }

        private MutexHandle handle;

        /// <inheritdoc/>
        public override bool WaitOne()
        {
            var code = MutexPrimitives.LockMutex(handle);
            if (code != ThreadResultCode.Success)
            {
                throw new Exception("Mutex lock acquisition failed with error code '" + code + "'.");
            }
            return true;
        }

        /// <summary>
        /// Releases the mutex.
        /// </summary>
        public void ReleaseMutex()
        {
            var code = MutexPrimitives.UnlockMutex(handle);
            if (code != ThreadResultCode.Success)
            {
                throw new Exception("Mutex lock release failed with error code '" + code + "'.");
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool explicitDisposing)
        {
            var code = MutexPrimitives.DisposeMutex(handle);
            if (code != ThreadResultCode.Success)
            {
                if (explicitDisposing)
                {
                    throw new Exception("Mutex disposal failed with error code '" + code + "'.");
                }
            }
        }
    }
}