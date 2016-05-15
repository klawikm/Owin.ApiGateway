namespace Owin.ApiGateway.Configuration
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class RoutingEndpoint
    {
        public RoutingEndpoint()
        {
            this.Urls = new List<string>();
        }

        public string Id { get; set; }

        public List<string> Urls { get; private set; }
    }
}