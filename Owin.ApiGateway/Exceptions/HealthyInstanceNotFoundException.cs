namespace Owin.ApiGateway.Exceptions
{
    using System;

    using Owin.ApiGateway.Configuration;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    [Serializable]
    public class HealthyInstanceNotFoundException : Exception
    {
        public HealthyInstanceNotFoundException(string endpointId)
            : base(string.Format("Healthy instance not found for endpoint with id = {0}. Healthy instance is the one with Status = {1}", endpointId, InstanceStatuses.Up))
        {
            this.EndpointId = endpointId;
        }

        protected HealthyInstanceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
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