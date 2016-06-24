namespace Owin.ApiGateway.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using Owin.ApiGateway.RoutingConditions;

    [XmlInclude(typeof(SoapActionCondition))]
    [XmlInclude(typeof(RequestPathAndQueryCondition))]
    [XmlInclude(typeof(AlwaysMatchingCondition))]
    public class RouteConfiguration
    {
        public AlwaysMatchingCondition AlwaysMatchingCondition { get; set; }

        public SoapActionCondition SoapActionCondition { get; set; }

        public RequestPathAndQueryCondition RequestPathAndQueryCondition { get; set; }

        internal RoutingCondition Condition
        {
            get {
                var configuredConditions = new List<RoutingCondition>();
                if (this.AlwaysMatchingCondition != null)
                {
                    configuredConditions.Add(this.AlwaysMatchingCondition);
                }

                if (this.SoapActionCondition != null)
                {
                    configuredConditions.Add(this.SoapActionCondition);
                }

                if (this.RequestPathAndQueryCondition != null)
                {
                    configuredConditions.Add(this.RequestPathAndQueryCondition);
                }

                return configuredConditions.First();
            }

            set
            {
                if (value is AlwaysMatchingCondition)
                {
                    this.AlwaysMatchingCondition = (AlwaysMatchingCondition)value;
                }

                if (value is SoapActionCondition)
                {
                    this.SoapActionCondition = (SoapActionCondition)value;
                }

                if (value is RequestPathAndQueryCondition)
                {
                    this.RequestPathAndQueryCondition = (RequestPathAndQueryCondition)value;
                }

                throw new Exception("Not supported condition type: " + value.GetType().FullName);
            }
        }

        public CacheConfiguration Cache { get; set; }

        public LoggerConfiguration Logger { get; set; }

        public string EndpointId { get; set; }
    }
}