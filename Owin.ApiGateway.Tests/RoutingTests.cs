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

        [TestMethod]
        public async Task SoapRequest_RoutingTableConfiguredAndEndpointInstanceUrlSufixDefined_ValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                var soapString = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Query>?</Query></s:Body></s:Envelope>";

                var client = server.HttpClient;

                client.DefaultRequestHeaders.Add("SOAPAction", "owin.apigateway.tests.action6");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                using (var response = await client.PostAsync("/service6", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService6, soapResponse);
                }
            }
        }

        [TestMethod]
        public async Task SoapRequest_MoreThanOneSoapActionInRoutingCondition_ValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                var soapString = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Query>?</Query></s:Body></s:Envelope>";

                var client = server.HttpClient;

                client.DefaultRequestHeaders.Add("SOAPAction", "owin.apigateway.tests.action_B");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                using (var response = await client.PostAsync("/service4", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService4, soapResponse);
                }
            }
        }

        [TestMethod]
        public async Task SoapRequest_MoreThanOneSoapActionPatternInRoutingCondition_ValidResponseReturned()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                var soapString = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Query>?</Query></s:Body></s:Envelope>";

                var client = server.HttpClient;

                client.DefaultRequestHeaders.Add("SOAPAction", "owin.apigateway.tests.action_Y123");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");
                using (var response = await client.PostAsync("/service5", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService5, soapResponse);
                }
            }
        }

        private class TestStartup
        {
            public const string SuccessfulResponseContentFromService2 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\"><a>Computer 1</a></SomeResponse></env:Body></env:Envelope>";
            public const string SuccessfulResponseContentFromService4 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\">Test4</SomeResponse></env:Body></env:Envelope>";
            public const string SuccessfulResponseContentFromService5 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\">Test5</SomeResponse></env:Body></env:Envelope>";
            public const string SuccessfulResponseContentFromService6 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\">Test6</SomeResponse></env:Body></env:Envelope>";

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
                    var responseFromService1 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService1.Content = new StringContent("Hello world from service1");

                    return responseFromService1;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service2.com/requestPath"), () =>
                {
                    var responseFromService2 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService2.Content = new StringContent(SuccessfulResponseContentFromService2);

                    return responseFromService2;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service3.com/1/2/3"), () =>
                {
                    var responseFromService3 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService3.Content = new StringContent("Hello world from service3");

                    return responseFromService3;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service4.com/requestPath"), () =>
                {
                    var responseFromService4 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService4.Content = new StringContent(SuccessfulResponseContentFromService4);

                    return responseFromService4;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service5.com/requestPath"), () =>
                {
                    var responseFromService5 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService5.Content = new StringContent(SuccessfulResponseContentFromService5);

                    return responseFromService5;
                });

                responseHandler.AddFakeResponseGenerator(new System.Uri("http://service6.com/requestPath/someSubPath"), () =>
                {
                    var responseFromService6 = new HttpResponseMessage(HttpStatusCode.OK);
                    responseFromService6.Content = new StringContent(SuccessfulResponseContentFromService6);

                    return responseFromService6;
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

                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service4",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service4.com/requestPath"
                                }
                            }
                        }
                    });

                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service5",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service5.com/requestPath"
                                }
                            }
                        }
                    });

                    configuration.Endpoints.Add(new Configuration.RoutingEndpoint
                    {
                        Id = "service6",
                        Instances = new Configuration.Instances
                        {
                            Instance = new System.Collections.Generic.List<Configuration.Instance>
                            {
                                new Configuration.Instance
                                {
                                    Status = ApiGateway.Configuration.InstanceStatuses.Up,
                                    Url = "http://service6.com/requestPath"
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
                            RequiredSoapActions = new[] { "owin.apigateway.tests.action1" }
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
                    // condition with more than one soap action:
                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapActions = new[] { "owin.apigateway.tests.action_A", "owin.apigateway.tests.action_B" }
                        },
                        EndpointId = "service4"
                    });
                    // condition with more than one soap action pattern (regex)
                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapActionRegexStrings = new[] { "^(owin.apigateway.tests.action_X)[0-9]*", "^(owin.apigateway.tests.action_Y)[0-9]*" }
                        },
                        EndpointId = "service5"
                    });
                    // condition with UrlSufix defined. UrlSufix modifies URL that was defined for endpoint
                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapActions = new[] { "owin.apigateway.tests.action6" }
                        },
                        EndpointId = "service6",
                        EndpointInstanceUrlSufix = "/someSubPath"
                    });
                }

                return configuration;
            }
        }
    }
}
