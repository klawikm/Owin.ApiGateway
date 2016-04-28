namespace Owin.ApiGateway
{
    using System.Xml.Serialization;

    using Owin.ApiGateway.RoutingConditions;

    [XmlInclude(typeof(SoapActionCondition))]
    [XmlInclude(typeof(RequestPathAndQueryCondition))]
    [XmlInclude(typeof(AlwaysMatchingCondition))]
    public class RoutingConditionEndpoint
    {
        public RoutingCondition Condition { get; set; }

        public string EndpointId { get; set; }
    }
}