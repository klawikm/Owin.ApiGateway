namespace Owin.ApiGateway
{
    public static class Exts
    {
        public static void UseProxy(this IAppBuilder app, IRoutingService routingService, ProxyOptions options = null)
        {
            options = options ?? new ProxyOptions();
            app.Use<ProxyMiddleware>(options, routingService);
        }
    }
}