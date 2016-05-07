namespace Owin.ApiGateway.Configuration
{
    public class CacheConfiguration
    {
        public bool IsEnabled { get; set; }

        public int ExpirationTimeInMinutes { get; set; }

        public bool DisableForGetMethod { get; set; }

        public bool DisableForDeleteMethod { get; set; }

        public bool DisableForHeadMethod { get; set; }

        public bool DisableForOptionsMethod { get; set; }

        public bool DisableForPostMethod { get; set; }

        public bool DisableForPutMethod { get; set; }

        public bool DisableForTraceMethod { get; set; }
    }
}
