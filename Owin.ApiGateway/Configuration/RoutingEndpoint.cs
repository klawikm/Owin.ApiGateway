namespace Owin.ApiGateway.Configuration
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class RoutingEndpoint
    {
        public RoutingEndpoint()
        {
            this.Instances = new Instances();
        }

        public string Id { get; set; }

        public Instances Instances { get; set; }
    }
}