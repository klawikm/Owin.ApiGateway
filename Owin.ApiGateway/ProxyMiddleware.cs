namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Hosting;

    using global::Common.Logging;

    using Owin.ApiGateway.Exceptions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class ProxyMiddleware
    {
        private readonly AppFunc next;

        private readonly ProxyOptions options;

        private readonly ILog logger;

        private readonly HttpMessageHandler httpClientMessageHandler;

        /// <summary>
        /// Initializes ProxyMiddleware. A middleware that sends request to target service, reads reponse from this service and sends it back to pipeline. This middleware should be the 
        /// last one in pipeline
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger">logger used to log warnings, errors and debug messages</param>
        /// <param name="options">proxy opitions</param>
        /// <param name="httpClientMessageHandler">if defined this message handler will be used to initialize HttpClient. This parameter is used in unit tests.</param>
        public ProxyMiddleware(AppFunc next, ILog logger, ProxyOptions options, HttpMessageHandler httpClientMessageHandler = null)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
            this.httpClientMessageHandler = httpClientMessageHandler;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                var inStream = env["owin.RequestBody"] as Stream;
                var outStream = env["owin.ResponseBody"] as Stream;
                var requestHeadersDictionary = env["owin.RequestHeaders"] as IDictionary<string, string[]>;
                var requestMethod = (string)env["owin.RequestMethod"];

                var requestUri = env[Tools.RedirectionUriEnvKey] as string;
                if (requestUri == null)
                {
                    throw new PipelineConfigurationException(string.Format(Tools.PossibleLackOfRoutingManagerInPipelineExceptionMessageTemplate, Tools.RedirectionUriEnvKey));
                }

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
                    using (var request = new HttpRequestMessage(new HttpMethod(requestMethod), requestUri))
                    {
                        if (!requestMethod.Equals("GET"))
                        {
                            request.Content = new StreamContent(inStream);
                        }

                        this.SetHttpHeaders(request, requestHeadersDictionary, requestUri);

                        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        var responseStream = await response.Content.ReadAsStreamAsync();

                        // because it is not possible to know the lenght of responseStream I have to copy its content to the buffer
                        // and read buffer size
                        var bufferStream = new MemoryStream();
                        await responseStream.CopyToAsync(bufferStream);
                        var bufferLength = bufferStream.Length;
                        bufferStream.Position = 0;

                        this.RememberResponseHeaders(env, response);
                        this.SetResponseHeaders(env, response);

                        // This is required to fix bug that occures when ApiGateway is hosted in IIS and response is sent in chunks
                        // It looks like chunks are not supported in hosted environment
                        bool generateChunks = HostingEnvironment.IsHosted && response.Headers.TransferEncodingChunked.HasValue
                                              && response.Headers.TransferEncodingChunked.Value;

                        if (generateChunks)
                        {
                            var sw = new StreamWriter(outStream);
                            sw.Write("{0:X}\r\n", bufferLength);
                            sw.Flush();
                        }

                        // await responseStream.CopyToAsync(outStream);
                        await bufferStream.CopyToAsync(outStream);

                        if (generateChunks)
                        {
                            var sw = new StreamWriter(outStream);
                            sw.Write("\r\n"); // CRLF to mark end of the last chunk
                            sw.Write("0\r\n"); // generate terminating chunk 
                            sw.Write("\r\n");
                            sw.Flush();
                        }
                    }
                }
                finally
                {
                    if (httpClient != null)
                    {
                        httpClient.Dispose();
                    }
                }

                if (this.options.VerboseMode)
                {
                    Console.Write(".");
                }
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat("Exception in ProxyMiddleware", ex);
                Tools.ShowExceptionDetails(ex);

                throw;
            }
        }

        private void SetHttpHeaders(HttpRequestMessage outgoingRequest, IDictionary<string, string[]> incomingRequestHeadersDictionary, string requestUri)
        {
            outgoingRequest.Headers.Clear();

            if (outgoingRequest.Content != null)
            {
                outgoingRequest.Content.Headers.Clear();
            }

            // TODO: Host header is handled in incorrectly
            Uri myUri = new Uri(requestUri);
            string host = myUri.Host;

            // TODO: Probably response headers are handled incorrectly. Check if headers are rewritten to the client

            foreach (var headerKey in incomingRequestHeadersDictionary.Keys)
            {
                var headerValues = incomingRequestHeadersDictionary[headerKey];

                if (headerKey.Equals("Host"))
                {
                    headerValues[0] = host;
                }

                if (headerKey.StartsWith("Content-"))
                {
                    outgoingRequest.Content.Headers.Add(headerKey, headerValues);
                }
                else
                {
                    outgoingRequest.Headers.Add(headerKey, headerValues);
                }
            }
        }

        private void RememberResponseHeaders(IDictionary<string, object> env, HttpResponseMessage response)
        {
            // Save response headers. These headers will be saved in cache if cache is enabled
            env[Tools.ResponseHeadersCollectionEnvKey] = response.Headers;

            if (response.Content != null)
            {
                env[Tools.ResponseContentHeadersCollectionEnvKey] = response.Content.Headers;
            }
        }

        /// <summary>
        /// Process response headers received from target service
        /// </summary>
        /// <param name="env"></param>
        /// <param name="response">Represents response received from target service</param>
        private void SetResponseHeaders(IDictionary<string, object> env, HttpResponseMessage response)
        {
            // Headers that will be sent to the client who called API Gateway
            var responseHeadersDictionary = env["owin.ResponseHeaders"] as IDictionary<string, string[]>;

            Tools.SetResponseHeaders(response.Headers, response.Content?.Headers, responseHeadersDictionary);
        }
    }
}