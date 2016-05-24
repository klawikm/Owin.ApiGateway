namespace Owin.ApiGateway.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using Owin.ApiGateway.Common;
    using Owin.ApiGateway.Common.Messages;
    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.Models;

    public class ConfigurationController : ApiController
    {
        private readonly IBus bus;

        private readonly IConfigurationProvider configurationProvider;

        public ConfigurationController(IBus bus, IConfigurationProvider configurationProvider)
        {
            this.bus = bus;
            this.configurationProvider = configurationProvider;
        }

        public IHttpActionResult GetCurrentConfiguration()
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;
            return Ok(config);
        }

        [HttpPost]
        public IHttpActionResult OverrideCurrentConfiguration(Configuration config)
        {
            this.SaveAndNotify(config);

            return Ok(config);
        }

        [HttpPost]
        public IHttpActionResult AddServiceInstance(AddInstanceRequest request)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;
            var endpoint = config.Endpoints.FirstOrDefault(e => e.Id.Equals(request.EndpointId));

            if (endpoint != null)
            {
                endpoint.Instances.Instance.Add(new Instance {Url = request.InstanceUrl, Status = InstanceStatuses.Up });

                this.SaveAndNotify(config);

                return this.Ok();
            }

            return this.NotFound();
        }

        private void SaveAndNotify(Configuration config)
        {
            this.configurationProvider.Save(config);
            this.bus.Publish(new ConfigurationChanged());
        }

        [HttpDelete]
        public IHttpActionResult RemoveServiceInstance(RemoveInstanceRequest request)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;
            var endpoint = config.Endpoints.FirstOrDefault(e => e.Id.Equals(request.EndpointId));

            if (endpoint != null)
            {
                var instance = endpoint.Instances.Instance.FirstOrDefault(i => i.Url.Equals(request.InstanceUrl));

                if (instance != null)
                {
                    endpoint.Instances.Instance.Remove(instance);

                    this.SaveAndNotify(config);
                    return this.Ok();
                }
            }

            return this.NotFound();
        }

        [HttpPut]
        public IHttpActionResult UpdateServiceInstance(UpdateInstanceRequest request)
        {
            var config = Owin.ApiGateway.Configuration.Configuration.Current;
            var endpoint = config.Endpoints.FirstOrDefault(e => e.Id.Equals(request.EndpointId));

            if (endpoint != null)
            {
                var instance = endpoint.Instances.Instance.FirstOrDefault(i => i.Url.Equals(request.InstanceUrl));

                if (instance != null)
                {
                    instance.Status = request.NewStatus;

                    this.SaveAndNotify(config);
                    return this.Ok();
                }
            }

            return this.NotFound();
        }
    }
}
