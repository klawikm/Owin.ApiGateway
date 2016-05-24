namespace Owin.ApiGateway
{
    using System;
    using System.Configuration;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Ninject;
    using Ninject.Web.Common.OwinHost;
    using Ninject.Web.WebApi.OwinHost;

    using Owin.ApiGateway.BusProvider.Rebus;
    using Owin.ApiGateway.Cache;
    using Owin.ApiGateway.Common;
    using Owin.ApiGateway.Common.Messages;
    using Owin.ApiGateway.Configuration.Providers;
    using Owin.ApiGateway.DbConfigurationStorageService;

    public class Startup
    {
        private static StandardKernel kernel;

        public Startup()
        {
            CreateKernel();
        }

        public void Configuration(IAppBuilder app)
        {
            // Setup WebAPI configuration  
            var configuration = new HttpConfiguration();
            configuration.Routes.Add("API Default", new HttpRoute("api/{Controller}/{action}"));

            // Configure OWIN pipeline

            // Configure NInject for WebAPI
            app.UseNinjectMiddleware(CreateKernel).UseNinjectWebApi(configuration);

            // Register the WebAPI to the pipeline. It is required for Configuration WebAPI 
            app.UseWebApi(configuration);

            var config = Owin.ApiGateway.Configuration.Configuration.Current;
            app.UseConfigurationManager(config);
            app.UseCache(new MemoryCacheProvider());
            app.UseRoutingManagerMiddleware(config);
            app.UseProxy(new ProxyOptions { VerboseMode = false });

            configuration.Formatters.Clear();
            configuration.Formatters.Add(new JsonMediaTypeFormatter());
            configuration.Formatters.JsonFormatter.SerializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static StandardKernel CreateKernel()
        {
            if (kernel != null)
            {
                return kernel;
            }

            kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            // kernel.Bind<IBus>().To<FakeBus>().InSingletonScope();
            kernel.Bind<IBus>().To<RebusProvider>().InSingletonScope();
            kernel.Bind<IConfigurationStorageService>().To<ConfigurationStorageService>();
            kernel.Bind<Configuration.IConfigurationProvider>().To<XmlConfigurationProvider>();

            IBus bus = kernel.Get<IBus>();
            ConfigureBus(bus);

            return kernel;
        }

        private static void ConfigureBus(IBus bus)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;

            var gatewayId = string.Format("{0} ({1})", Assembly.GetEntryAssembly().GetName().Name, System.Environment.MachineName);

            if (ConfigurationManager.AppSettings["gatewayId"] != null)
            {
                string overridenGatewayId = ConfigurationManager.AppSettings["gatewayId"];

                Console.WriteLine("Using gatewayId from App.config ({0}) instead of automatically generated ({1})", overridenGatewayId, gatewayId);
                gatewayId = overridenGatewayId;
            }

            bus.Initialize(gatewayId);
            bus.RegisterHandler<ConfigurationChanged>(HandleConfigurationChangedMessage);
            bus.Subscribe<ConfigurationChanged>();
        }

        private static Task HandleConfigurationChangedMessage(ConfigurationChanged message)
        {
            Console.WriteLine();
            Console.WriteLine("[!] Received message ConfigurationChanged.");

            // Reload configuration
            Owin.ApiGateway.Configuration.Configuration.Load();

            return Task.FromResult(0);
        }
    }
}