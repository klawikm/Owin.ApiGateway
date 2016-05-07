namespace Owin.ApiGateway
{
    using Owin.ApiGateway.Cache;

    public static class Exts
    {
        public static void UseConfigurationManager(this IAppBuilder app, Configuration.Configuration configuration)
        {
            app.Use<ConfigurationManagerMiddleware>(configuration);
        }

        public static void UseCache(this IAppBuilder app, ICache cache)
        {
            app.Use<CacheMiddleware>(cache);
        }

        public static void UseRoutingManagerMiddleware(this IAppBuilder app, Configuration.Configuration configuration)
        {
            app.Use<RoutingManagerMiddleware>(configuration);
        }

        public static void UseProxy(this IAppBuilder app, ProxyOptions options = null)
        {
            options = options ?? new ProxyOptions();
            app.Use<ProxyMiddleware>(options);
        }
    }
}