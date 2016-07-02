namespace Owin.ApiGateway
{
    using System;

    using global::Common.Logging;

    using Owin.ApiGateway.Cache;
    using Logger;
    using System.Net.Http;
    public static class Exts
    {
        public static void UseConfigurationManager(this IAppBuilder app, Func<Configuration.Configuration> configurationProvider, ILog logger)
        {
            app.Use<ConfigurationManagerMiddleware>(configurationProvider, logger);
        }

        public static void UseCache(this IAppBuilder app, ICache cache, ILog logger)
        {
            app.Use<CacheMiddleware>(cache, logger);
        }

        public static void UseRequestResponseLogger(this IAppBuilder app, ILog logger, IRequestResponseLogger requestResponseLogger)
        {
            app.Use<LoggerMiddleware>(logger, requestResponseLogger);
        }

        public static void UseRoutingManagerMiddleware(this IAppBuilder app, ILog logger, Func<Configuration.Configuration> configurationProvider)
        {
            app.Use<RoutingManagerMiddleware>(configurationProvider, logger);
        }

        public static void UseProxy(this IAppBuilder app, ILog logger, ProxyOptions options = null, HttpMessageHandler httpClientMessageHandler = null)
        {
            options = options ?? new ProxyOptions();

            if (httpClientMessageHandler == null)
            {
                app.Use<ProxyMiddleware>(logger, options);
            }
            else
            {
                app.Use<ProxyMiddleware>(logger, options, httpClientMessageHandler);
            }
        }
    }
}