namespace Owin.ApiGateway.Configuration
{
    using System.Collections.Generic;
    using System.Web.Hosting;

    using Owin.ApiGateway.Common;
    using Owin.ApiGateway.Configuration.Providers;
    using Owin.ApiGateway.RoutingConditions;

    public class Configuration
    {
        public static Configuration Current { get; set; }

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
            var re = new RoutingEndpoint { Id = endpointId };

            re.Instances.Instance.Add(new Instance {Url = endpointUri, Status = InstanceStatuses.Up });
            
            this.Endpoints.Add(re);
        }

        public void AddRoute(RoutingCondition condition, string endpointId)
        {
            this.Routes.Add(new RouteConfiguration { Condition = condition, EndpointId = endpointId });
        }

        public static Configuration Load()
        {
            var configurationFileName = "Configuration.xml";
            
            // fix path is APIGateway is hosted in IIS
            if (HostingEnvironment.IsHosted)
            {
                configurationFileName = HostingEnvironment.MapPath("~/" + configurationFileName);
            }
            
            IConfigurationStorageService configurationStorageService = new LocalFilesystemConfigurationStorageService(configurationFileName);
            //IConfigurationProvider configurationProvider = new YamlConfigurationProvider("Configuration.yaml");
            IConfigurationProvider configurationProvider = new XmlConfigurationProvider(configurationStorageService);
            Current = configurationProvider.Load(); 
            
            /*
            var dbConfigStorageService = new DbConfigurationStorageService.ConfigurationStorageService();
            IConfigurationProvider configurationProvider2 = new XmlConfigurationProvider(dbConfigStorageService);
            Current = configurationProvider2.Load();
            */
            
            return Current;
        }
    }
}