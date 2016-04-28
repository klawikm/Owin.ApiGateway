namespace Owin.ApiGateway.Configuration
{
    using System.Collections.Generic;

    using Owin.ApiGateway.RoutingConditions;

    public class Configuration
    {
        public Configuration()
        {
            this.Endpoints = new List<RoutingEndpoint>();
            this.ConditionEndpoints = new List<RoutingConditionEndpoint>();
        }

        public List<RoutingEndpoint> Endpoints { get; set; }

        public List<RoutingConditionEndpoint> ConditionEndpoints { get; set; }

        public void AddEndpoint(string endpointId, string endpointUri)
        {
            this.Endpoints.Add(new RoutingEndpoint { Id = endpointId, Uri = endpointUri });
        }

        public void AddRoute(RoutingCondition condition, string endpointId)
        {
            this.ConditionEndpoints.Add(new RoutingConditionEndpoint { Condition = condition, EndpointId = endpointId });
        }
    }
}