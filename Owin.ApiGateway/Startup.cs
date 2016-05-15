namespace Owin.ApiGateway
{
    using Owin.ApiGateway.Cache;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;

            app.UseConfigurationManager(config);
            app.UseCache(new MemoryCacheProvider());
            app.UseRoutingManagerMiddleware(config);
            app.UseProxy(new ProxyOptions { VerboseMode = false });
        }
    }
}