namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    using Owin.ApiGateway.Cache;
    using Owin.ApiGateway.Exceptions;
    using Owin.ApiGateway.RoutingConditions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class CacheMiddleware
    {
        private readonly AppFunc next;

        private readonly ICache cache;

        public CacheMiddleware(AppFunc next, ICache cache)
        {
            this.next = next;
            this.cache = cache;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                var routeConfiguration = env[Tools.RouteConfigurationEnvKey] as RouteConfiguration;

                if (routeConfiguration == null)
                {
                    throw new PipelineConfigurationException(
                        string.Format(Tools.PossibleLackOfConfigurationManagerInPipelineExceptionMessageTemplate, Tools.RouteConfigurationEnvKey));
                }

                if (routeConfiguration.Cache != null && routeConfiguration.Cache.IsEnabled)
                {
                    var cacheConfig = routeConfiguration.Cache;

                    var context = new OwinContext(env);

                    // buffer the request body
                    var requestBuffer = new MemoryStream();
                    await context.Request.Body.CopyToAsync(requestBuffer);

                    requestBuffer.Position = 0;
                    var cacheKey = this.CreateKey(env, requestBuffer);
                    Console.WriteLine("Cache key: {0}", cacheKey);

                    // check if we have request and response in the cache
                    var cachedResponseBody = this.cache.GetFromCache(cacheKey) as string;
                    if (cachedResponseBody != null)
                    {
                        var bytesFromCachedResponse = Encoding.UTF8.GetBytes(cachedResponseBody);
                        context.Response.Body.Write(bytesFromCachedResponse, 0, bytesFromCachedResponse.Length);

                        return;
                    }

                    requestBuffer.Position = 0;
                    context.Request.Body = requestBuffer;

                    // buffer the response body
                    var originalResponseStream = context.Response.Body;
                    var responseBuffer = new MemoryStream();
                    context.Response.Body = responseBuffer;

                    await this.next(env);

                    responseBuffer.Position = 0;
                    var responseBufferReader = new StreamReader(responseBuffer);
                    var responseBody = await responseBufferReader.ReadToEndAsync();

                    // write reponse to cache
                    this.cache.SetInCache(cacheKey, responseBody, new TimeSpan(hours: 0, minutes: cacheConfig.ExpirationTimeInMinutes, seconds: 0));

                    // We need to do this so that the response we buffered is flushed out to the client application.
                    responseBuffer.Position = 0;
                    await responseBuffer.CopyToAsync(originalResponseStream);

                    return;
                }

            }
            catch (Exception ex)
            {
                Tools.ShowExceptionDetails(ex);

                throw;
            }

            await this.next(env);
        }

        public string CreateKey(IDictionary<string, object> env, Stream requestStream)
        {
            var keyBuilder = new StringBuilder();

            var requestPath = (string)env["owin.RequestPath"];
            var requestQuery = (string)env["owin.RequestQueryString"];

            keyBuilder.Append(requestPath);
            keyBuilder.Append("_");
            keyBuilder.Append(requestQuery);
            keyBuilder.Append("_");

            string soapAction;

            if (Tools.TryGetSoapAction(env, out soapAction))
            {
                keyBuilder.Append(soapAction);
                keyBuilder.Append("_");
            }
            
            keyBuilder.Append(GetHashedKey(requestStream));

            return keyBuilder.ToString();
        }
        
        private static string GetHashedKey(Stream inputStream)
        {
            var md5 = MD5.Create();
            var output = Convert.ToBase64String(md5.ComputeHash(inputStream));
            // inputStream.Position = 0;

            return output;
        }
    }
}
