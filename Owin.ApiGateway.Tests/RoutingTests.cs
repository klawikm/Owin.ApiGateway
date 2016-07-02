namespace Owin.ApiGateway.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using Microsoft.Owin.Testing;
    using System.Net.Http;
    using Owin.ApiGateway;
    using global::Common.Logging;
    using Moq;
    using System.Net;
    [TestClass]
    public class RoutingTests
    {
        [TestMethod]
        public async Task HttpGetRequest_RoutingTableConfigured_ValidResponseReturnedFromTargetService()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/service1");
                var responseString = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Hello world from service1", responseString);
            }
        }

        private class TestStartup
        {
            private static Configuration.Configuration configuration;

            public void Configuration(IAppBuilder app)
            {
                var logMock = new Mock<ILog>();
                ILog logger = logMock.Object;

                app.UseConfigurationManager(this.GetCurrentConfiguration, logger);
                app.UseRoutingManagerMiddleware(logger, this.GetCurrentConfiguration);

                var responseHandler = new FakeResponseHandler();
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("Hello world from service1");
                responseHandler.AddFakeResponse(new System.Uri("http://service1.com/requestPath"), response);
                app.UseProxy(logger, new ProxyOptions { VerboseMode = false }, responseHandler);
            }

            private Configuration.Configuration GetCurrentConfiguration()
            {
                if (configuration == null)
                {
                    configuration = new Configuration.Configuration();
                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service1",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service1.com/requestPath"
                                }
                            }
                        }
                    });

                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        RequestPathAndQueryCondition = new RoutingConditions.RequestPathAndQueryCondition
                        {
                            RequestPathRegexString = "^service1(.*)"
                        },
                        EndpointId = "service1"
                    });
                }

                return configuration;
            }
        }
    }
}
