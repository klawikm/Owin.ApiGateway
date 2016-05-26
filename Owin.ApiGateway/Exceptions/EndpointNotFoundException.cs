namespace Owin.ApiGateway.Exceptions
{
    using System;

    public class EndpointNotFoundException : Exception
    {
        public EndpointNotFoundException(string endpointId)
            : base(string.Format("Endpoint with id = {0} was not found", endpointId))
        {
            this.EndpointId = endpointId;
        }

        public string EndpointId { get; private set; }
    }
}