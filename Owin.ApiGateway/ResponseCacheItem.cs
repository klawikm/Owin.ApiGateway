namespace Owin.ApiGateway
{
    using System.Net.Http.Headers;

    public class ResponseCacheItem
    {
        public byte[] ResponseBodyArray { get; set; }
        
        public HttpResponseHeaders ResponseHeaders { get; set; }

        public HttpContentHeaders ResponseContentHeaders { get; set; }
    }
}
