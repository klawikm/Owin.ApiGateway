namespace Owin.ApiGateway.Logger
{
    using global::Common.Logging;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    /// <summary>
    /// Saves log data to the give log store using background thread. The main idea was to not affect service processing time so logging job 
    /// is done in the background..
    /// </summary>
    internal class BackgroundThreadLogger : IRequestResponseLogger
    {
        #region Constants and Fields

        /// <summary>
        /// The max queue semaphore.
        /// </summary>
        private static readonly Semaphore MaxQueueSemaphore = new Semaphore(MaxQueueLength, MaxQueueLength);

        /// <summary>
        /// The queue.
        /// </summary>
        private static readonly Queue<LogEntry> Queue = new Queue<LogEntry>();

        /// <summary>
        /// The worker.
        /// </summary>
        private static Thread worker;

        /// <summary>
        /// WorkerLockObject
        /// </summary>
        private static readonly object WorkerLockObject = new object();

        #endregion

        #region Constructors and Destructors
        internal BackgroundThreadLogger(IRequestResponseLogStoreWriter requestResponseLogStoreWriter, ILog messageLogger)
        {
            this.messageLogger = messageLogger;
            this.requestResponseLogStoreWriter = requestResponseLogStoreWriter;
        }
        #endregion

        #region Properties

        private ILog messageLogger;

        private IRequestResponseLogStoreWriter requestResponseLogStoreWriter;

        private static int? maxQueueLength;

        /// <summary>
        /// Gets the max queue length.
        /// </summary>
        public static int MaxQueueLength
        {
            get
            {
                if (maxQueueLength.HasValue)
                {
                    return maxQueueLength.Value;
                }

                string maxQueueLengthFromConfig = ConfigurationManager.AppSettings["WebServiceLogger_MaxQueueLength"];

                maxQueueLength = string.IsNullOrEmpty(maxQueueLengthFromConfig) ? 1000 : int.Parse(maxQueueLengthFromConfig);

                return maxQueueLength.Value;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// The work.
        /// </summary>
        public void Work()
        {
            try
            {
                for (;;)
                {
                    var logEntry = DequeueLogEntry();

                    if (logEntry != null)
                    {
                        this.requestResponseLogStoreWriter.SaveLogData(logEntry);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                this.messageLogger.Error("BackgroundThreadLogger: Error was thrown when saving data to log store", e);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Takes message from queue method.
        /// </summary>
        /// <returns>
        /// The <see cref="DBMessageInfo"/>.
        /// </returns>
        private LogEntry DequeueLogEntry()
        {
            string mode = ConfigurationManager.AppSettings["WebServiceLogger_ExpiringWorkerThreadBehavior"];

            lock (Queue)
            {
                for (;;)
                {
                    if (Queue.Count > 0)
                    {
                        var logMessage = Queue.Dequeue();
                        MaxQueueSemaphore.Release();

                        return logMessage;
                    }

                    if (String.IsNullOrEmpty(mode) || mode.ToUpper().Equals("N"))
                    {
                        Monitor.Wait(Queue);
                    }
                    else
                    {
                        Monitor.Wait(Queue, TimeSpan.FromSeconds(10));

                        // if Queue.Count is 0 then timeout occured
                        if (Queue.Count == 0)
                        {
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The initialize method.
        /// </summary>
        private void Init()
        {
            lock (WorkerLockObject)
            {
                if (worker == null)
                {
                    worker = new Thread(Work);
                }

                if (worker.ThreadState == ThreadState.Stopped)
                {
                    worker = new Thread(Work);
                }

                if (!worker.IsAlive)
                {
                    worker.Start();
                }
            }
        }

        /// <summary>
        /// The enqueue log message.
        /// </summary>
        /// <param name="logMessage">
        /// The log message.
        /// </param>
        public void EnqueueLogMessage(LogEntry logEntry)
        {
            if (MaxQueueSemaphore.WaitOne(1000))
            {
                Init();

                lock (Queue)
                {
                    Queue.Enqueue(logEntry);
                    Monitor.Pulse(Queue);
                }
            }
            else
            {
                this.messageLogger.WarnFormat("BackgroundThreadLogger: Timed-out enqueueing a LogMessage. Queue size = {0}", Queue.Count);
            }
        }

        #endregion
    }

}
