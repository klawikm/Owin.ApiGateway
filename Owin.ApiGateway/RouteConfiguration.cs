namespace Owin.ApiGateway
{
    using System.Xml.Serialization;

    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.RoutingConditions;

    [XmlInclude(typeof(SoapActionCondition))]
    [XmlInclude(typeof(RequestPathAndQueryCondition))]
    [XmlInclude(typeof(AlwaysMatchingCondition))]
    public class RouteConfiguration
    {
        public RoutingCondition Condition { get; set; }

        public CacheConfiguration Cache { get; set; }

        public string EndpointId { get; set; }
    }
}