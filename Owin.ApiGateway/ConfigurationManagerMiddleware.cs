namespace Owin.ApiGateway
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class ConfigurationManagerMiddleware
    {
        private readonly AppFunc next;

        private readonly Configuration.Configuration configuration;

        public ConfigurationManagerMiddleware(AppFunc next, Configuration.Configuration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                this.LoadRouteConfiguration(env);
            }
            catch (Exception ex)
            {
                Tools.ShowExceptionDetails(ex);

                throw;
            }

            await this.next(env);
        }

        private void LoadRouteConfiguration(IDictionary<string, object> env)
        {
            foreach (var routeConfiguration in this.configuration.Routes)
            {
                var conditionResult = routeConfiguration.Condition.Check(env);
                if (conditionResult.Success)
                {
                    env.Add(Tools.RouteConfigurationEnvKey, routeConfiguration);
                    env.Add(Tools.ConditionCaptureGroupsEnvKey, conditionResult.CaptureGroups);

                    return;
                }
            }

            var requestInfo = Tools.BuildRequestInfo(env);
            throw new MatchingRoutingConditionNotFoundException(string.Format("Request info.={0}", requestInfo));
        }
    }
}
