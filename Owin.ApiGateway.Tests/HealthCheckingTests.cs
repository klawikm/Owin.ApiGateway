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
    public class HealthCheckingTests
    {
        [TestMethod]
        public async Task HttpGetRequest_AllInstancesAreDown_500ResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/service1");
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

                var responseString = await response.Content.ReadAsStringAsync();
                Assert.IsNotNull(responseString, "Response string should contain info message");
                Assert.IsTrue(responseString.Contains("All instances serving"), "Response should contain string: 'All instances serving...'");
                Assert.IsTrue(responseString.Contains("All instances serving"), "Response should contain string: 'are down'");
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

                var responseHandler = PrepareHttpMessageHandler();
                app.UseProxy(logger, new ProxyOptions { VerboseMode = false }, responseHandler);
            }

            private HttpMessageHandler PrepareHttpMessageHandler()
            {
                var responseHandler = new FakeResponseHandler();

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service1.com/requestPath"), () =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.OK);
                    r.Content = new StringContent("Hello world from service1 instance 1");

                    return r;
                });

                return responseHandler;
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
                                    Status = ApiGateway.Configuration.InstanceStatuses.Down,
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
