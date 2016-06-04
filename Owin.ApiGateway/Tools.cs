namespace Owin.ApiGateway
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Text;

    public static class Tools
    {
        public const string RouteConfigurationEnvKey = "Owin.ApiGateway.RouteConfiguration";

        public const string ConditionCaptureGroupsEnvKey = "Owin.ApiGateway.CaptureGroups";

        public const string RedirectionUriEnvKey = "Owin.ApiGateway.RediectionUri";

        public const string ResponseHeadersCollectionEnvKey = "Owin.ApiGateway.ResponseHeadersCollection";

        public const string ResponseContentHeadersCollectionEnvKey = "Owin.ApiGateway.ResponseContentHeadersCollection";

        public const string PossibleLackOfConfigurationManagerInPipelineExceptionMessageTemplate = "Environment should contain {0}. Check if you have ConfigurationManager in your pipeline";

        public const string PossibleLackOfRoutingManagerInPipelineExceptionMessageTemplate = "Environment should contain {0}. Check if you have RoutingManager in your pipeline";

        public static bool TryGetSoapAction(IDictionary<string, object> env, out string soapAction)
        {
            var requestHeadersDictionary = env["owin.RequestHeaders"] as IDictionary<string, string[]>;

            var headerKey = "SOAPAction";
            if (!requestHeadersDictionary.ContainsKey(headerKey))
            {
                soapAction = null;
                return false;
            }

            soapAction = requestHeadersDictionary["SOAPAction"].FirstOrDefault();

            return true;
        }
        
        public static void ShowExceptionDetails(Exception ex)
        {
            Console.WriteLine(ex.GetType().ToString());
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        
        public static string BuildRequestInfo(IDictionary<string, object> env)
        {
            var infoBuilder = new StringBuilder();

            var requestPath = (string)env["owin.RequestPath"];
            var requestQuery = (string)env["owin.RequestQueryString"];

            infoBuilder.Append(requestPath);

            if (!string.IsNullOrEmpty(requestQuery))
            {
                infoBuilder.AppendFormat("?{0}", requestQuery);
            }

            string soapAction;
            if (Tools.TryGetSoapAction(env, out soapAction))
            {
                infoBuilder.AppendFormat(" [SoapAction = {0}]", soapAction);
            }

            return infoBuilder.ToString();
        }
        
        public static void SetResponseHeaders(HttpResponseHeaders responseHeadersFromTargetService,
            HttpContentHeaders responseContentHeadersFromTargetService,
            IDictionary<string, string[]> responseHeadersDictionary)
        {
            foreach (var header in responseHeadersFromTargetService)
            {
                // without this condition an exception is thrown:
                // "The 'Keep-Alive' header must be modified using the appropriate property or method.\r\nParameter name: name"
                if (header.Key.Equals("Keep-Alive"))
                {
                    continue;
                }

                responseHeadersDictionary[header.Key] = header.Value.ToArray();
            }

            if (responseContentHeadersFromTargetService != null)
            {
                foreach (var header in responseContentHeadersFromTargetService)
                {
                    responseHeadersDictionary[header.Key] = header.Value.ToArray();
                }
            }
        }

    }
}