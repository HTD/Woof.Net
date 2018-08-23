using System;
using System.Threading;

namespace Woof.Net {

    /// <summary>
    /// Multi-thread semaphore allows multiple threads to block until release signal occurs.
    /// </summary>
    public class MultiThreadSemaphore : IDisposable {

        /// <summary>
        /// Gets or sets the highest number of threads allowed to be blocked.
        /// When this number is exceeded, blocking another thread will cause releasing the oldest thread.
        /// </summary>
        public int MaxThreadsAllowed { get; set; } = 16;

        /// <summary>
        /// Event source used to block threads until external activation occurs.
        /// </summary>
        private readonly AutoResetEvent WaitEventSource = new AutoResetEvent(false);


        /// <summary>
        /// The number of waiting threads.
        /// </summary>
        private volatile int WaitingThreadsCount;

        /// <summary>
        /// Releases all resources used by the current instance of the System.Threading.WaitHandle class.
        /// </summary>
        public void Dispose() => WaitEventSource.Dispose();

        /// <summary>
        /// Blocks the current thread until release signal occurs.
        /// </summary>
        public void WaitEvent() {
            Interlocked.Increment(ref WaitingThreadsCount);
            if (WaitingThreadsCount > MaxThreadsAllowed) WaitEventSource.Set();
            WaitEventSource.WaitOne();
            Interlocked.Decrement(ref WaitingThreadsCount);
            
        }

        /// <summary>
        /// Release all blocked threads.
        /// </summary>
        public void ReleaseAll() {
            while (WaitingThreadsCount > 0) WaitEventSource.Set();
        }

    }

}