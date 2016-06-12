namespace Owin.ApiGateway
{
    using global::Common.Logging;

    using Owin.ApiGateway.Cache;

    public static class Exts
    {
        public static void UseConfigurationManager(this IAppBuilder app, Configuration.Configuration configuration, ILog logger)
        {
            app.Use<ConfigurationManagerMiddleware>(configuration, logger);
        }

        public static void UseCache(this IAppBuilder app, ICache cache, ILog logger)
        {
            app.Use<CacheMiddleware>(cache, logger);
        }

        public static void UseRoutingManagerMiddleware(this IAppBuilder app, ILog logger, Configuration.Configuration configuration)
        {
            app.Use<RoutingManagerMiddleware>(configuration, logger);
        }

        public static void UseProxy(this IAppBuilder app, ILog logger, ProxyOptions options = null)
        {
            options = options ?? new ProxyOptions();
            app.Use<ProxyMiddleware>(logger, options);
        }
    }
}