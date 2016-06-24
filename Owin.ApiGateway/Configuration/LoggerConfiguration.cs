namespace Owin.ApiGateway.Configuration
{
    public class LoggerConfiguration
    {
        public bool IsEnabled { get; set; }
        public bool? LogRequests { get; set; }
        public bool? LogResponses { get; set; }
    }
}
