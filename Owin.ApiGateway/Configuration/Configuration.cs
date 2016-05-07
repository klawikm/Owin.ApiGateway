namespace Owin.ApiGateway.Configuration
{
    using System.Collections.Generic;

    using Owin.ApiGateway.Configuration.Providers;
    using Owin.ApiGateway.RoutingConditions;

    public class Configuration
    {
        public Configuration()
        {
            this.Endpoints = new List<RoutingEndpoint>();
            this.Routes = new List<RouteConfiguration>();
        }

        public int Port { get; set; }

        public List<RoutingEndpoint> Endpoints { get; set; }

        public List<RouteConfiguration> Routes { get; set; }

        public void AddEndpoint(string endpointId, string endpointUri)
        {
            this.Endpoints.Add(new RoutingEndpoint { Id = endpointId, Uri = endpointUri });
        }

        public void AddRoute(RoutingCondition condition, string endpointId)
        {
            this.Routes.Add(new RouteConfiguration { Condition = condition, EndpointId = endpointId });
        }

        public static Configuration Load()
        {
            //IConfigurationProvider configurationProvider = new YamlConfigurationProvider("Configuration.yaml");
            IConfigurationProvider configurationProvider = new XmlConfigurationProvider("Configuration.xml");
            return configurationProvider.Load();
        }
    }
}