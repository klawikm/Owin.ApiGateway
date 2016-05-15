namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.Exceptions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class RoutingManagerMiddleware
    {
        private readonly AppFunc next;

        private readonly Configuration.Configuration configuration;
        

        public RoutingManagerMiddleware(AppFunc next, Configuration.Configuration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                var routeConfiguration = env[Tools.RouteConfigurationEnvKey] as RouteConfiguration;
                var conditionCaptureGroups = env[Tools.ConditionCaptureGroupsEnvKey] as IList<KeyValuePair<string, string>>;

                if (routeConfiguration == null)
                {
                    throw new PipelineConfigurationException(string.Format(Tools.PossibleLackOfConfigurationManagerInPipelineExceptionMessageTemplate, Tools.RouteConfigurationEnvKey));
                }

                if (conditionCaptureGroups == null)
                {
                    throw new PipelineConfigurationException(string.Format(Tools.PossibleLackOfConfigurationManagerInPipelineExceptionMessageTemplate, Tools.ConditionCaptureGroupsEnvKey));
                }

                var endpoint = this.configuration.Endpoints.FirstOrDefault(e => e.Id.Equals(routeConfiguration.EndpointId));

                if (endpoint == null)
                {
                    throw new EndpointNotFountException(routeConfiguration.EndpointId);
                }

                var endpointUriBuilder = new StringBuilder(endpoint.Urls.First());
                if (conditionCaptureGroups.Count > 0)
                {
                    foreach (var captureGroups in conditionCaptureGroups)
                    {
                        endpointUriBuilder.Replace(captureGroups.Key, captureGroups.Value);
                    }
                }

                env.Add(Tools.RedirectionUriEnvKey, endpointUriBuilder.ToString());
            }
            catch (Exception ex)
            {
                Tools.ShowExceptionDetails(ex);

                throw;
            }

            await this.next(env);
        }
    }
}
