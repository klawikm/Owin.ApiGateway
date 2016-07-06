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
    using Cache;
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public async Task HttpGetRequest_CacheIsEnabled_FirstResponseComesFromTargetServiceButSecondResponseComesFromCache()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                HttpResponseMessage response = await server.HttpClient.GetAsync("/service1");
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.AreEqual("Hello world from service1 instance 1", responseString);
                Assert.AreEqual(1, TestStartup.responseHandler.GetNumberOfCalls("http://instance1.service1.com/requestPath"));

                response = await server.HttpClient.GetAsync("/service1");
                responseString = await response.Content.ReadAsStringAsync();
                Assert.AreEqual("Hello world from service1 instance 1", responseString);
                Assert.AreEqual(1, TestStartup.responseHandler.GetNumberOfCalls("http://instance1.service1.com/requestPath"), "response sould be taken from cache. It is why counter should be equal 1");
            }
        }

        [TestMethod]
        public async Task SoapRequest_CacheIsEnabled_FirstResponseComesFromTargetServiceButSecondResponseComesFromCache()
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
                Assert.AreEqual(1, TestStartup.responseHandler.GetNumberOfCalls("http://service2.com/requestPath"));

                // repeat call. This call should not reach target service. Response should be returned from cache
                content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                using (var response = await client.PostAsync("/service2", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService2, soapResponse);
                }
                Assert.AreEqual(1, TestStartup.responseHandler.GetNumberOfCalls("http://service2.com/requestPath"), "response sould be taken from cache. It is why counter should be equal 1");
            }
        }

        private class TestStartup
        {
            private static Configuration.Configuration configuration;

            public static FakeResponseHandler responseHandler;

            public const string SuccessfulResponseContentFromService2 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\"><a>Computer 1</a></env:Body></env:Envelope>";

            public void Configuration(IAppBuilder app)
            {
                var logMock = new Mock<ILog>();
                ILog logger = logMock.Object;

                app.UseConfigurationManager(this.GetCurrentConfiguration, logger);
                app.UseCache(new MemoryCacheProvider(), logger);
                app.UseRoutingManagerMiddleware(logger, this.GetCurrentConfiguration);

                responseHandler = PrepareHttpMessageHandler();
                app.UseProxy(logger, new ProxyOptions { VerboseMode = false }, responseHandler);
            }

            private FakeResponseHandler PrepareHttpMessageHandler()
            {
                var responseHandler = new FakeResponseHandler();

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://instance1.service1.com/requestPath"), () =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.OK);
                    r.Content = new StringContent("Hello world from service1 instance 1");

                    return r;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service2.com/requestPath"), () =>
                {
                    var responseFromService2 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService2.Content = new StringContent(SuccessfulResponseContentFromService2);

                    return responseFromService2;
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
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://instance1.service1.com/requestPath"
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

                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        RequestPathAndQueryCondition = new RoutingConditions.RequestPathAndQueryCondition
                        {
                            RequestPathRegexString = "^service1(.*)"
                        },
                        Cache = new ApiGateway.Configuration.CacheConfiguration
                        {
                            IsEnabled = true,
                            ExpirationTimeInMinutes = 1
                        },
                        EndpointId = "service1"
                    });

                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapAction = "owin.apigateway.tests.action1"
                        },
                        Cache = new ApiGateway.Configuration.CacheConfiguration
                        {
                            IsEnabled = true,
                            ExpirationTimeInMinutes = 1
                        },
                        EndpointId = "service2"
                    });
                }

                return configuration;
            }
        }
    }
}
