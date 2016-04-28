namespace Owin.ApiGateway
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Owin.ApiGateway.Exceptions;
    using Owin.ApiGateway.RoutingConditions;

    public class StaticRoutingService : IRoutingService
    {
        private readonly List<RoutingConditionEndpoint> conditionEndpoints = new List<RoutingConditionEndpoint>();

        private readonly List<RoutingEndpoint> endpoints = new List<RoutingEndpoint>();

        public string GetRedirectionBaseUri(IDictionary<string, object> env)
        {
            foreach (var conditionEndpoint in this.conditionEndpoints)
            {
                var conditionResult = conditionEndpoint.Condition.Check(env);
                if (conditionResult.Success)
                {
                    var endpoint = this.endpoints.FirstOrDefault(e => e.Id.Equals(conditionEndpoint.EndpointId));

                    if (endpoint == null)
                    {
                        throw new EndpointNotFountException(conditionEndpoint.EndpointId);
                    }

                    var endpointUriBuilder = new StringBuilder(endpoint.Uri);
                    if (conditionResult.CaptureGroups.Count > 0)
                    {
                        foreach (var captureGroups in conditionResult.CaptureGroups)
                        {
                            endpointUriBuilder.Replace(captureGroups.Key, captureGroups.Value);
                        }
                    }

                    return endpointUriBuilder.ToString();
                }
            }

            var requestInfo = this.BuildRequestInfo(env);
            throw new MatchingRoutingConditionNotFoundException(string.Format("Request info.={0}", requestInfo));
        }

        private string BuildRequestInfo(IDictionary<string, object> env)
        {
            var infoBuilder = new StringBuilder();

            var requestPath = (string)env["owin.RequestPath"];
            var requestQuery = (string)env["owin.RequestQueryString"];

            infoBuilder.Append(requestPath);

            if (!string.IsNullOrEmpty(requestQuery))
            {
                infoBuilder.AppendFormat("?{0}", requestQuery);
            }

            string soapAction;
            if (Tools.TryGetSoapAction(env, out soapAction))
            {
                infoBuilder.AppendFormat(" [SoapAction = {0}]", soapAction);
            }

            return infoBuilder.ToString();
        }

        public void AddEndpoint(string endpointId, string endpointUri)
        {
            this.endpoints.Add(new RoutingEndpoint { Id = endpointId, Uri = endpointUri });
        }

        public void AddRoute(RoutingCondition condition, string endpointId)
        {
            this.conditionEndpoints.Add(new RoutingConditionEndpoint { Condition = condition, EndpointId = endpointId });
        }
    }
}