namespace Owin.ApiGateway.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::Common.Logging;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using Configuration;
    using Exceptions;
    using Microsoft.Owin;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Web.Hosting;
    public class LoggerMiddleware
    {
        private readonly AppFunc next;

        private readonly ILog logger;

        private readonly IRequestResponseLogger requestResponseLogger;

        public LoggerMiddleware(AppFunc next, ILog logger, IRequestResponseLogger requestResponseLogger)
        {
            this.next = next;
            this.logger = logger;
            this.requestResponseLogger = requestResponseLogger;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var startDateTime = DateTime.Now;

            try
            {
                var routeConfiguration = env[Tools.RouteConfigurationEnvKey] as RouteConfiguration;

                if (routeConfiguration == null)
                {
                    throw new PipelineConfigurationException(
                        string.Format(Tools.PossibleLackOfConfigurationManagerInPipelineExceptionMessageTemplate, Tools.RouteConfigurationEnvKey));
                }

                if (routeConfiguration.Logger != null && routeConfiguration.Logger.IsEnabled)
                {
                    var cacheConfig = routeConfiguration.Cache;

                    var context = new OwinContext(env);

                    // buffer the request body
                    var requestBuffer = new MemoryStream();
                    await context.Request.Body.CopyToAsync(requestBuffer);
                    requestBuffer.Position = 0;

                    byte[] requestArray = requestBuffer.ToArray();
                    string requestString = System.Text.Encoding.UTF8.GetString(requestArray);

                    var requestHeaders = context.Request.Headers;

                    requestBuffer.Position = 0;
                    context.Request.Body = requestBuffer;

                    // buffer the response body
                    var originalResponseStream = context.Response.Body;
                    var responseBuffer = new MemoryStream();
                    context.Response.Body = responseBuffer;

                    await this.next(env);

                    var responseHeaders = context.Response.Headers;

                    responseBuffer.Position = 0;
                    byte[] responseArray = responseBuffer.ToArray();

                    // We need to do this so that the response we buffered is flushed out to the client application.
                    responseBuffer.Position = 0;
                    await responseBuffer.CopyToAsync(originalResponseStream);

                    var logEntry = this.BuildLogEntry(startDateTime, env,  requestString, requestHeaders, responseArray, responseHeaders);
                    this.requestResponseLogger.EnqueueLogMessage(logEntry);

                    return;
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat("Exception in LoggerMiddleware.", ex);
                Tools.ShowExceptionDetails(ex);

                throw;
            }

            await this.next(env);
        }

        private LogEntry BuildLogEntry(DateTime startDateTime, IDictionary<string, object> env, string requestString, IHeaderDictionary requestHeaders, byte[] responseArray, IHeaderDictionary responseHeaders)
        {
            double processingTimeInMS = (DateTime.Now - startDateTime).TotalMilliseconds;

            bool isFromCache;
            if (env.ContainsKey(Tools.IsFromCacheEnvKey))
            {
                isFromCache = (bool)env[Tools.IsFromCacheEnvKey];
            }
            else
            {
                isFromCache = false;
            }

            string soapAction = null;
            Tools.TryGetSoapAction(requestHeaders, out soapAction);

            string contentEncodingMethod;
            bool isResponseGziped = false;
            if (Tools.TryGetContentEncoding(responseHeaders, out contentEncodingMethod))
            {
                isResponseGziped = contentEncodingMethod.Equals("gzip");
            }

            // This is required to fix bug that occures when ApiGateway is hosted in IIS and response is sent in chunks
            // It looks like chunks are not supported in hosted environment
            string transferEncoding;
            bool isChunkedTransferEncoding = false;
            if (Tools.TryGetTransferEncoding(responseHeaders, out transferEncoding))
            {
                isChunkedTransferEncoding = transferEncoding.ToLower().Equals("chunked");
            }

            string requestHeadersString = this.BuildHeadersString(requestHeaders);
            string responseHeadersString = this.BuildHeadersString(responseHeaders);

            string requestPath = (string)env["owin.RequestPath"];
            string requestQuery = (string)env["owin.RequestQueryString"];
            string requestedUrl = requestPath + requestQuery;

            return new LogEntry
            {
                DateTime = startDateTime,
                RequestedUrl = requestedUrl,
                SoapAction = soapAction,
                RequestString = requestString,
                RequestHeaders = requestHeadersString,
                ResponseArray = responseArray,
                ResponseHeaders = responseHeadersString,
                ResponseTimeInMS = (int)processingTimeInMS,
                IsFromCache = isFromCache,
                IsResponseGziped = isResponseGziped,
                IsChunkedTransferEncoding = isChunkedTransferEncoding
            };
        }

        private string BuildHeadersString(IHeaderDictionary requestHeaders)
        {
            var sb = new StringBuilder();

            foreach (string key in requestHeaders.Keys)
            {
                var values = requestHeaders.GetValues(key);

                if (values != null)
                {
                    sb.AppendLine($"{key}: {string.Join(",", values)}");
                }
            }

            return sb.ToString();
        }
    }
}
