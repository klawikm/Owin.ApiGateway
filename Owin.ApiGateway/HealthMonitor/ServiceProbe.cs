namespace Owin.ApiGateway.HealthMonitor
{
    using System;
    using System.Threading;

    using global::Common.Logging;

    public class ServiceProbe : IServiceProbe
    {
        private const int IntervalMilliseconds = 60 * 1000;

        /// <summary>
        /// The worker.
        /// </summary>
        private static Thread worker;

        private static readonly object WorkerLockObject = new object();
        
        private Func<Configuration.Configuration> configuartionProvider;

        private ILog logger;

        public void Initialize(Func<Configuration.Configuration> configuartionProvider, ILog logger)
        {
            this.configuartionProvider = configuartionProvider;
            this.logger = logger;
        }

        public void Start()
        {
            if (this.configuartionProvider == null)
            {
                throw new Exception($"{typeof(ServiceProbe).Name} is not initialized. You have to call method Initialize(...) first");
            }
        }

        public void Stop()
        {
            
        }

        private void InitializeWorker()
        {
            lock (WorkerLockObject)
            {
                if (worker == null)
                {
                    worker = new Thread(this.Work);
                }

                if (worker.ThreadState == ThreadState.Stopped)
                {
                    worker = new Thread(this.Work);
                }

                if (!worker.IsAlive)
                {
                    worker.Start();
                }
            }
        }

        private void Work()
        {
            try
            {
                for (;;)
                {
                    var config = this.configuartionProvider();

                    // TODO: check endpoints status. 

                    Thread.Sleep(IntervalMilliseconds);
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                this.logger.Error("Exception was thrown when checking service health", e);
            }
        }
    }
}
