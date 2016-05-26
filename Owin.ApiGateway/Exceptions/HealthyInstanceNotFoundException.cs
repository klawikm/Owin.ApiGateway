namespace Owin.ApiGateway.Exceptions
{
    using System;

    using Owin.ApiGateway.Configuration;

    public class HealthyInstanceNotFoundException : Exception
    {
        public HealthyInstanceNotFoundException(string endpointId)
            : base(string.Format("Healthy instance not found for endpoint with id = {0}. Healthy instance is the one with Status = {1}", endpointId, InstanceStatuses.Up))
        {
            this.EndpointId = endpointId;
        }

        public string EndpointId { get; private set; }
    }
}