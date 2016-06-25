namespace Owin.ApiGateway.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    [Serializable]
    public class EndpointNotFoundException : Exception
    {
        public EndpointNotFoundException(string endpointId)
            : base(string.Format("Endpoint with id = {0} was not found", endpointId))
        {
            this.EndpointId = endpointId;
        }

        protected EndpointNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.EndpointId = info.GetString("EndpointId");
        }


        public string EndpointId { get; private set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("EndpointId", this.EndpointId);
        }
    }
}