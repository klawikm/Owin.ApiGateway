﻿namespace Owin.ApiGateway
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using global::Common.Logging;

    using Microsoft.Owin;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class ConfigurationManagerMiddleware
    {
        private readonly AppFunc next;

        private readonly Func<Configuration.Configuration> configurationProvider;

        private readonly ILog logger;

        public ConfigurationManagerMiddleware(AppFunc next, Func<Configuration.Configuration> configurationProvider, ILog logger)
        {
            this.next = next;
            this.configurationProvider = configurationProvider;
            this.logger = logger;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                if (this.LoadRouteConfiguration(env))
                {
                    await this.next(env);
                }
                else
                {
                    var context = new OwinContext(env);
                    var resp = context.Response;
                    resp.StatusCode = 404;

                    var requestInfo = Tools.BuildRequestInfo(env);
                    resp.ReasonPhrase = "Routing configuration not found";

                    using (var sw = new StreamWriter(resp.Body))
                    {
                        sw.WriteLine("<html lang=\"en\">");
                        sw.WriteLine("</body>");
                        sw.WriteLine($"Routing configuration not found for {requestInfo}. Check configuration.");
                        sw.WriteLine("</body>");
                        sw.WriteLine("</html>");

                        sw.Flush();
                    }
                }
            }
            catch (HealthyInstanceNotFoundException hinf_ex)
            {
                var context = new OwinContext(env);
                var resp = context.Response;
                resp.StatusCode = 500;

                var requestInfoString = Tools.BuildRequestInfo(env);
                resp.Write($"All instances serving [{requestInfoString}] are down.");
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat("Exception in ConfigurationManagerMiddleware", ex);
                Tools.ShowExceptionDetails(ex);

                throw;
            }
            
        }

        private bool LoadRouteConfiguration(IDictionary<string, object> env)
        {
            var configuration = this.configurationProvider();
            foreach (var routeConfiguration in configuration.Routes)
            {
                var conditionResult = routeConfiguration.Condition.Check(env);
                if (conditionResult.Success)
                {
                    env.Add(Tools.RouteConfigurationEnvKey, routeConfiguration);
                    env.Add(Tools.ConditionCaptureGroupsEnvKey, conditionResult.CaptureGroups);

                    return true;
                }
            }

            //var requestInfo = Tools.BuildRequestInfo(env);
            //throw new MatchingRoutingConditionNotFoundException(string.Format("Request info.={0}", requestInfo));

            return false;
        }
    }
}
