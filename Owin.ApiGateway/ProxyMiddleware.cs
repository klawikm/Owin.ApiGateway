namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Owin.ApiGateway.Exceptions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class ProxyMiddleware
    {
        private readonly AppFunc next;

        private readonly ProxyOptions options;

        public ProxyMiddleware(AppFunc next, ProxyOptions options)
        {
            this.next = next;
            this.options = options;
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

                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(requestMethod), requestUri))
                    {
                        if (!requestMethod.Equals("GET"))
                        {
                            request.Content = new StreamContent(inStream);
                        }

                        this.SetHttpHeaders(request, requestHeadersDictionary);

                        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        var responseStream = await response.Content.ReadAsStreamAsync();

                        await responseStream.CopyToAsync(outStream);
                    }
                }

                if (this.options.VerboseMode)
                {
                    Console.Write(".");
                }
            }
            catch (Exception ex)
            {
                Tools.ShowExceptionDetails(ex);

                throw;
            }
        }

        private void SetHttpHeaders(HttpRequestMessage outgoingRequest, IDictionary<string, string[]> incomingRequestHeadersDictionary)
        {
            outgoingRequest.Headers.Clear();

            if (outgoingRequest.Content != null)
            {
                outgoingRequest.Content.Headers.Clear();
            }

            foreach (var headerKey in incomingRequestHeadersDictionary.Keys)
            {
                if (headerKey.StartsWith("Content-"))
                {
                    outgoingRequest.Content.Headers.Add(headerKey, incomingRequestHeadersDictionary[headerKey]);
                }
                else
                {
                    outgoingRequest.Headers.Add(headerKey, incomingRequestHeadersDictionary[headerKey]);
                }
            }
        }
    }
}