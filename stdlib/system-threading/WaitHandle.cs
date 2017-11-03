namespace System.Threading
{
    /// <summary>
    /// A handle on which one can wait.
    /// </summary>
    public abstract class WaitHandle : IDisposable
    {
        // NOTE: this WaitHandle implementation is non-conforming.
        // 'WaitOne()' and 'Dispose(bool)' are abstract instead of
        // virtual.
        //
        // I'm not sure if that's such a big deal, though. It shouldn't
        // really matter from a user's perspective and inheriting directly
        // from WaitHandle in user code is usually rather useless.

        protected WaitHandle()
        {
            isDisposed = false;
        }

        ~WaitHandle()
        {
            if (!isDisposed)
            {
                Dispose(false);
            }
        }

        private bool isDisposed;

        public abstract bool WaitOne();

        protected abstract void Dispose(bool explicitDisposing);

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            isDisposed = true;
            Dispose(true);
        }
    }
}