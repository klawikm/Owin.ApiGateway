namespace Owin.ApiGateway.Models
{
    using Owin.ApiGateway.Configuration;

    public class UpdateInstanceRequest
    {
        public string EndpointId { get; set; }

        public string InstanceUrl { get; set; }

        public InstanceStatuses NewStatus { get; set; }
    }
}
