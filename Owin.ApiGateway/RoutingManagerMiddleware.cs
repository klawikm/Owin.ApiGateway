namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using global::Common.Logging;

    using Owin.ApiGateway.Configuration;
    using Owin.ApiGateway.Exceptions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class RoutingManagerMiddleware
    {
        private readonly AppFunc next;

        private readonly Func<Configuration.Configuration> configurationProvider;

        private readonly ILog logger;

        private static Dictionary<string, string> _endpointId2LastUrlTemplate = new Dictionary<string, string>();
        

        public RoutingManagerMiddleware(AppFunc next, Func<Configuration.Configuration> configurationProvider, ILog logger)
        {
            this.next = next;
            this.configurationProvider = configurationProvider;
            this.logger = logger;
        }

        public static void ResetRoundRobinData()
        {
            _endpointId2LastUrlTemplate = new Dictionary<string, string>();
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

                var configuration = this.configurationProvider();
                var endpoint = configuration.Endpoints.FirstOrDefault(e => e.Id.Equals(routeConfiguration.EndpointId));

                if (endpoint == null)
                {
                    throw new EndpointNotFoundException(routeConfiguration.EndpointId);
                }

                string endpointUrlTemplateString = null;

                if (_endpointId2LastUrlTemplate.ContainsKey(endpoint.Id))
                {
                    string lastUriTemplate = _endpointId2LastUrlTemplate[endpoint.Id];
                    int indexOfLastUri = endpoint.Instances.Instance.FindIndex(i => i.Url.Equals(lastUriTemplate));

                    if (indexOfLastUri != -1 && (indexOfLastUri + 1) < endpoint.Instances.Instance.Count)
                    {
                        for (int j = indexOfLastUri + 1; j < endpoint.Instances.Instance.Count; j++)
                        {
                            if (endpoint.Instances.Instance[j].Status == InstanceStatuses.Up)
                            {
                                endpointUrlTemplateString = endpoint.Instances.Instance[j].Url;
                            }
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(endpointUrlTemplateString))
                {
                    endpointUrlTemplateString = endpoint.Instances.Instance.FirstOrDefault(i => i.Status == InstanceStatuses.Up)?.Url;
                }

                if (string.IsNullOrEmpty(endpointUrlTemplateString))
                {
                    _endpointId2LastUrlTemplate.Remove(endpoint.Id);

                    throw new HealthyInstanceNotFoundException(endpoint.Id);
                }

                _endpointId2LastUrlTemplate[endpoint.Id] = endpointUrlTemplateString;

                var endpointUriBuilder = new StringBuilder(endpointUrlTemplateString);
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
                this.logger.ErrorFormat("Exception in RoutingManagerMiddleware", ex);
                Tools.ShowExceptionDetails(ex);

                throw;
            }

            await this.next(env);
        }
    }
}
