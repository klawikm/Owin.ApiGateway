namespace Owin.ApiGateway.Models
{
    public class RemoveInstanceRequest
    {
        public string EndpointId { get; set; }

        public string InstanceUrl { get; set; }
    }
}
