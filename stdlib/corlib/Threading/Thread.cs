using System.Primitives.Threading;

namespace System.Threading
{
    public delegate void ThreadStart();

    /// <summary>
    /// Creates and controls a thread.
    /// </summary>
    public sealed class Thread
    {
        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="start">The function that is run when the thread is started.</param>
        public Thread(ThreadStart start)
        {
            this.entryPoint = entryPoint;
            this.IsAlive = false;
        }

        private ThreadStart entryPoint;
        private ThreadId id;

        private bool HasStarted => entryPoint == null;

        /// <summary>
        /// Tells if the thread is currently running.
        /// </summary>
        /// <returns><c>true</c> if the thread is running; otherwise, <c>false</c>.</returns>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// Starts executing the thread.
        /// </summary>
        public void Start()
        {
            if (HasStarted)
            {
                throw new InvalidOperationException("Thread instance has already been started.");
            }

            if (!ThreadingPrimitives.CreateThread(entryPoint.Invoke, out id))
            {
                throw new Exception("An error occurred while trying to start a new thread.");
            }
            entryPoint = null;
        }

        /// <summary>
        /// Joins this thread with the callee's thread.
        /// </summary>
        public void Join()
        {
            if (!IsAlive)
            {
                throw new InvalidOperationException("Thread instance is not running.");
            }
            IsAlive = false;

            if (!ThreadingPrimitives.JoinThread(id))
            {
                throw new Exception("An error occurred while trying to join threads.");
            }
        }
    }
}