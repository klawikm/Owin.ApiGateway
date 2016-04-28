namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class ProxyMiddleware
    {
        private readonly AppFunc next;

        private readonly ProxyOptions options;

        private readonly IRoutingService routingService;

        public ProxyMiddleware(AppFunc next, ProxyOptions options, IRoutingService routingService)
        {
            this.next = next;
            this.options = options;
            this.routingService = routingService;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                var inStream = env["owin.RequestBody"] as Stream;
                var outStream = env["owin.ResponseBody"] as Stream;
                var requestHeadersDictionary = env["owin.RequestHeaders"] as IDictionary<string, string[]>;
                var requestMethod = (string)env["owin.RequestMethod"];

                var requestUri = this.GetRedirectionUri(env, requestHeadersDictionary);

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
                ShowExceptionDetails(ex);

                throw;
            }
        }

        private static void ShowExceptionDetails(Exception ex)
        {
            Console.WriteLine(ex.GetType().ToString());
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        private string GetRedirectionUri(IDictionary<string, object> env, IDictionary<string, string[]> requestHeadersDictionary)
        {
            var requestPath = (string)env["owin.RequestPath"];
            var requestQueryString = (string)env["owin.RequestQueryString"];

            var requestUri = this.routingService.GetRedirectionBaseUri(env);

            var requestUriBuilder = new StringBuilder();
            requestUriBuilder.Append(requestUri);

            //if (!requestPath.Equals("/"))
            //{
            //    if (requestUri.EndsWith("/") && requestPath.StartsWith("/"))
            //    {
            //        requestPath = requestPath.Substring(1);
            //    }

            //    requestUriBuilder.Append(requestPath);
            //}

            //if (!string.IsNullOrEmpty(requestQueryString))
            //{
            //    requestUriBuilder.AppendFormat("?{0}", requestQueryString);
            //}

            return requestUriBuilder.ToString();
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