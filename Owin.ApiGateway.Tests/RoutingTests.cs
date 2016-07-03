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
    using System.Text;
    [TestClass]
    public class RoutingTests
    {
        [TestMethod]
        public async Task HttpGetRequest_RoutingTableConfigured_ValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/service1");
                var responseString = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Hello world from service1", responseString);
            }
        }

        [TestMethod]
        public async Task HttpGetRequestWithParametersInRequestPath_RoutingTableConfigured_ParametersWereParsedAndThenConvertedAndValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/service3/query_1_2_3"); // will be converted to http://service3.com/1/2/3
                var responseString = await response.Content.ReadAsStringAsync();

                Assert.AreEqual("Hello world from service3", responseString);
            }
        }

        [TestMethod]
        public async Task HttpGetRequest_RoutingTableNotConfigured_404ResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/serviceABC");
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode); 
            }
        }

        [TestMethod]
        public async Task SoapRequest_RoutingTableConfigured_ValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                var soapString = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Query>?</Query></s:Body></s:Envelope>";

                var client = server.HttpClient;
                
                client.DefaultRequestHeaders.Add("SOAPAction", "owin.apigateway.tests.action1");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                using (var response = await client.PostAsync("/service2", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService2, soapResponse);
                }
            }
        }

        private class TestStartup
        {
            public const string SuccessfulResponseContentFromService2 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\"><a>Computer 1</a></env:Body></env:Envelope>";

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

                var responseFromService1 = new HttpResponseMessage(HttpStatusCode.OK);
                responseFromService1.Content = new StringContent("Hello world from service1");
                responseHandler.AddFakeResponse(new System.Uri("http://service1.com/requestPath"), responseFromService1);

                var responseFromService2 = new HttpResponseMessage(HttpStatusCode.OK);
                responseFromService2.Content = new StringContent(SuccessfulResponseContentFromService2);
                responseHandler.AddFakeResponse(new System.Uri("http://service2.com/requestPath"), responseFromService2);

                var responseFromService3 = new HttpResponseMessage(HttpStatusCode.OK);
                responseFromService3.Content = new StringContent("Hello world from service3");
                responseHandler.AddFakeResponse(new System.Uri("http://service3.com/1/2/3"), responseFromService3);

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
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service1.com/requestPath"
                                }
                            }
                        }
                    });
                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service2",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service2.com/requestPath"
                                }
                            }
                        }
                    });
                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service3",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service3.com/{R:1}/{R:2}/{R:3}"
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
                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapAction = "owin.apigateway.tests.action1"
                        },
                        EndpointId = "service2"
                    });
                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        RequestPathAndQueryCondition = new RoutingConditions.RequestPathAndQueryCondition
                        {
                            RequestPathRegexString = "^service3/query_([0-9]*)_([0-9]*)_([0-9]*)"
                        },
                        EndpointId = "service3"
                    });
                }

                return configuration;
            }
        }
    }
}
