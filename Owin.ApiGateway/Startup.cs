namespace Owin.ApiGateway
{
    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.Configuration.Providers;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //IConfigurationProvider configurationProvider = new YamlConfigurationProvider("Configuration.yaml");
            IConfigurationProvider configurationProvider = new XmlConfigurationProvider("Configuration.xml");
            var config = configurationProvider.Load();

            var routingService = new StaticRoutingService();
            config.Endpoints.ForEach(e => routingService.AddEndpoint(e.Id, e.Uri));
            config.ConditionEndpoints.ForEach(ce => routingService.AddRoute(ce.Condition, ce.EndpointId));

            app.UseProxy(routingService, new ProxyOptions { VerboseMode = false });
        }
    }
}