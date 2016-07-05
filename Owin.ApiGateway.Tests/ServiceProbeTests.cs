using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.ApiGateway.HealthMonitor;
using System.Net.Http;
using System.Net;
using Moq;
using Common.Logging;
using System.Linq;

namespace Owin.ApiGateway.Tests
{
    [TestClass]
    public class ServiceProbeTests
    {
        // private static Configuration.Configuration configuration;

        [TestMethod]
        public void ServiceInstanceIsMarkedAsDown_MonitoringEndpointReturnsSuccessMessage_ServiceInstanceIsMarkedAsUp()
        {
            var configuration = new Configuration.Configuration();
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
                },
                HealthCheck = new Configuration.HealthCheckConfiguration
                {
                    MonitoringPath = "/status",
                    ResponseShouldContainString = "OK"
                }
            });

            // Arrange
            Func< Configuration.Configuration> configurationGen = () => {
                return configuration;
            };

            var logMock = new Mock<ILog>();
            ILog logger = logMock.Object;

            var responseHandler = new FakeResponseHandler();
            responseHandler.AddFakeResponseGenerator(new System.Uri("http://service1.com/status"), () =>
            {
                var r = new HttpResponseMessage(HttpStatusCode.OK);
                r.Content = new StringContent("Service is OK.");

                return r;
            });

            // Act
            var serviceProbe = new ServiceProbe(httpClientMessageHandler: responseHandler);
            serviceProbe.Initialize(configurationGen, logger);
            serviceProbe.TestEndpoint(configuration.Endpoints.First());

            // Assert
            Assert.AreEqual(Configuration.InstanceStatuses.Up, configuration.Endpoints.First().Instances.Instance.First().Status);
        }

        [TestMethod]
        public void ServiceInstanceIsMarkedAsUp_MonitoringEndpointReturnsErrorMessage_ServiceInstanceIsMarkedAsDown()
        {
            var configuration = new Configuration.Configuration();
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
                },
                HealthCheck = new Configuration.HealthCheckConfiguration
                {
                    MonitoringPath = "/status",
                    ResponseShouldContainString = "OK"
                }
            });

            // Arrange
            Func<Configuration.Configuration> configurationGen = () => {
                return configuration;
            };

            var logMock = new Mock<ILog>();
            ILog logger = logMock.Object;

            var responseHandler = new FakeResponseHandler();
            responseHandler.AddFakeResponseGenerator(new System.Uri("http://service1.com/status"), () =>
            {
                var r = new HttpResponseMessage(HttpStatusCode.InternalServerError);

                return r;
            });

            // Act
            var serviceProbe = new ServiceProbe(httpClientMessageHandler: responseHandler);
            serviceProbe.Initialize(configurationGen, logger);
            serviceProbe.TestEndpoint(configuration.Endpoints.First());

            // Assert
            Assert.AreEqual(Configuration.InstanceStatuses.Down, configuration.Endpoints.First().Instances.Instance.First().Status);
        }
    }
}
