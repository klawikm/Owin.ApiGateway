namespace Owin.ApiGateway.Exceptions
{
    using System;

    public class EndpointNotFountException : Exception
    {
        public EndpointNotFountException(string endpointId)
            : base(string.Format("Endpoint with id = {0} was not found", endpointId))
        {
            this.EndpointId = endpointId;
        }

        public string EndpointId { get; private set; }
    }
}