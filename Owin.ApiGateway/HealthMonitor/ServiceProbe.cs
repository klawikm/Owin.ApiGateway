﻿namespace Owin.ApiGateway.HealthMonitor
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using global::Common.Logging;

    using Owin.ApiGateway.Configuration;

    public class ServiceProbe : IServiceProbe
    {
        private const int IntervalMilliseconds = 60 * 1000;

        private const int TimeoutMilliseconds = 60 * 1000;

        /// <summary>
        /// The worker.
        /// </summary>
        private static Thread worker;

        private static readonly object WorkerLockObject = new object();

        private Func<Configuration> configuartionProvider;

        private ILog logger;

        private HttpMessageHandler httpClientMessageHandler = null;

        public ServiceProbe(HttpMessageHandler httpClientMessageHandler = null)
        {
            this.httpClientMessageHandler = httpClientMessageHandler;
        }

        public void Initialize(Func<Configuration> configuartionProvider, ILog logger)
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

            this.InitializeWorker();
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
                    try
                    {
                        var config = this.configuartionProvider();

                        if (config.Endpoints == null)
                        {
                            Thread.Sleep(IntervalMilliseconds);
                            continue;
                        }

                        foreach (var endpoint in config.Endpoints)
                        {
                            this.TestEndpoint(endpoint);
                        }

                        Thread.Sleep(IntervalMilliseconds);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Exception was thrown when checking service health", e);
                        Thread.Sleep(IntervalMilliseconds);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        public void TestEndpoint(RoutingEndpoint endpoint)
        {
            if (endpoint.Instances?.Instance == null)
            {
                return;
            }

            foreach (var instance in endpoint.Instances.Instance)
            {
                this.TestServiceInstance(endpoint, instance);
            }
        }

        private void TestServiceInstance(RoutingEndpoint endpoint, Instance instance)
        {
            var healthCheck = endpoint.HealthCheck;
            if (healthCheck == null)
            {
                return;
            }

            Uri u = new Uri(instance.Url);
            string host = u.Host;
            string monitoringUri = u.Scheme + "://" + host + ":" + u.Port + (healthCheck.MonitoringPath.StartsWith("/") ? "" : "/") + healthCheck.MonitoringPath;

            HttpClient httpClient;

            if (this.httpClientMessageHandler == null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                httpClient = new HttpClient(this.httpClientMessageHandler, disposeHandler: true);
            }

            try
            {
                using (var request = new HttpRequestMessage(new HttpMethod("get"), monitoringUri))
                {
                    var responseTask = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    responseTask.Wait(TimeoutMilliseconds);

                    if (responseTask.IsCanceled || responseTask.IsFaulted)
                    {
                        // timeout occured or an error
                        instance.Status = InstanceStatuses.Down;
                        return;
                    }

                    var response = responseTask.Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        instance.Status = InstanceStatuses.Down;
                        return;
                    }

                    if (response.IsSuccessStatusCode && string.IsNullOrEmpty(healthCheck.ResponseShouldContainString))
                    {
                        instance.Status = InstanceStatuses.Up;
                        return;
                    }

                    var readResponseStreamTask = response.Content.ReadAsStreamAsync();
                    readResponseStreamTask.Wait(TimeoutMilliseconds);

                    var responseStream = readResponseStreamTask.Result;

                    if (readResponseStreamTask.IsCanceled || readResponseStreamTask.IsFaulted)
                    {
                        instance.Status = InstanceStatuses.Down;
                        return;
                    }

                    using (var sr = new StreamReader(responseStream))
                    {
                        string responseString = sr.ReadToEnd();

                        if (responseString.Contains(healthCheck.ResponseShouldContainString))
                        {
                            instance.Status = InstanceStatuses.Up;
                            return;
                        }
                        else
                        {
                            instance.Status = InstanceStatuses.Down;
                            return;
                        }
                    }

                }
            }
            catch (Exception)
            {
                instance.Status = InstanceStatuses.Down;
            }
            finally
            {
                if (httpClient != null)
                {
                    httpClient.Dispose();
                }
            }
        } // TestServiceInstance 
    } // class
}
