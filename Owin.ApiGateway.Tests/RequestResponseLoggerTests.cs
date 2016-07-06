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
    using Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    [TestClass]
    public class RequestResponseLoggerTests
    {
        [TestMethod]
        public async Task HttpGetRequest_LoggerIsEnabled_RequestAndResponseWasInterceptedByLogger()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                TestStartup.requestResponseFakeLogger.logEntries.Clear();

                HttpResponseMessage response = await server.HttpClient.GetAsync("/service1");
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.AreEqual("Hello world from service1 instance 1", responseString);

                Assert.AreEqual(1, TestStartup.requestResponseFakeLogger.logEntries.Count);
                var loggedEntry = TestStartup.requestResponseFakeLogger.logEntries.First();
                Assert.IsNotNull(loggedEntry.RequestHeaders, "Request headers must be logged");
                Assert.IsNotNull(loggedEntry.ResponseHeaders, "Response headers must be logged");
                Assert.IsNotNull(loggedEntry.RequestedUrl, "RequestURL must be logged");
                Assert.IsNotNull(loggedEntry.ResponseArray, "Response must be logged");
            }
        }

        [TestMethod]
        public async Task SoapRequest_LoggerIsEnabled_RequestAndResponseWasInterceptedByLogger()
        {
            using (var server = TestServer.Create<TestStartup>())
            {
                TestStartup.requestResponseFakeLogger.logEntries.Clear();

                var soapString = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Query>?</Query></s:Body></s:Envelope>";

                var client = server.HttpClient;

                client.DefaultRequestHeaders.Add("SOAPAction", "owin.apigateway.tests.action1");
                var content = new StringContent(soapString, Encoding.UTF8, "text/xml");

                using (var response = await client.PostAsync("/service2", content))
                {
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    Assert.AreEqual(TestStartup.SuccessfulResponseContentFromService2, soapResponse);
                }

                Assert.AreEqual(1, TestStartup.requestResponseFakeLogger.logEntries.Count);
                var loggedEntry = TestStartup.requestResponseFakeLogger.logEntries.First();
                Assert.IsNotNull(loggedEntry.RequestHeaders, "Request headers must be logged");
                Assert.IsNotNull(loggedEntry.ResponseHeaders, "Response headers must be logged");
                Assert.IsNotNull(loggedEntry.RequestedUrl, "RequestURL must be logged");
                Assert.IsNotNull(loggedEntry.ResponseArray, "Response must be logged");
                Assert.IsNotNull(loggedEntry.RequestString, "Request string must be logged");
                Assert.IsNotNull(loggedEntry.SoapAction, "SoapAction must be logged");
            }
        }

        private class FakeLogger : IRequestResponseLogger
        {
            public List<LogEntry> logEntries = new List<LogEntry>();

            public void EnqueueLogMessage(LogEntry logEntry)
            {
                this.logEntries.Add(logEntry);
            }
        }

        private class TestStartup
        {
            private static Configuration.Configuration configuration;

            public static FakeResponseHandler responseHandler;

            public static FakeLogger requestResponseFakeLogger = new FakeLogger();

            public const string SuccessfulResponseContentFromService2 = "<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"><env:Body><SomeResponse \"xmlns=owin.apigateway.tests\"><a>Computer 1</a></env:Body></env:Envelope>";

            public void Configuration(IAppBuilder app)
            {
                var logMock = new Mock<ILog>();
                ILog logger = logMock.Object;

                app.UseConfigurationManager(this.GetCurrentConfiguration, logger);
                app.UseRequestResponseLogger(logger, requestResponseFakeLogger);
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
                        Logger = new ApiGateway.Configuration.LoggerConfiguration
                        {
                            IsEnabled = true,
                            LogRequests = true,
                            LogResponses = true
                        },
                        EndpointId = "service1"
                    });

                    configuration.Routes.Add(new Configuration.RouteConfiguration
                    {
                        SoapActionCondition = new RoutingConditions.SoapActionCondition
                        {
                            RequiredSoapAction = "owin.apigateway.tests.action1"
                        },
                        Logger = new ApiGateway.Configuration.LoggerConfiguration
                        {
                            IsEnabled = true,
                            LogRequests = true,
                            LogResponses = true
                        },
                        EndpointId = "service2"
                    });
                }

                return configuration;
            }
        }
    }
}
