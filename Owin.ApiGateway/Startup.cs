namespace Owin.ApiGateway
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Load();

            var routingService = new StaticRoutingService();
            config.Endpoints.ForEach(e => routingService.AddEndpoint(e.Id, e.Uri));
            config.ConditionEndpoints.ForEach(ce => routingService.AddRoute(ce.Condition, ce.EndpointId));

            app.UseProxy(routingService, new ProxyOptions { VerboseMode = false });
        }
    }
}