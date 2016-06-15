namespace Owin.ApiGateway.Configuration
{
    public class HealthCheckConfiguration
    {
        public string MonitoringPath { get; set; }

        public string ResponseShouldContainString { get; set; }
    }
}
