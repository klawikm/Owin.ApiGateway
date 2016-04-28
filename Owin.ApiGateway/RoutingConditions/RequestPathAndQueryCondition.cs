namespace Owin.ApiGateway.RoutingConditions
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public class RequestPathAndQueryCondition : RoutingCondition
    {
        private Regex requestPathRegex;

        private string requestPathRegexString;

        public RequestPathAndQueryCondition()
        {
        }

        public RequestPathAndQueryCondition(string requestPathRegexString)
        {
            this.RequestPathRegexString = requestPathRegexString;
        }

        public string RequestPathRegexString
        {
            get
            {
                return this.requestPathRegexString;
            }

            set
            {
                this.requestPathRegexString = value;
                this.requestPathRegex = new Regex(this.requestPathRegexString);
            }
        }

        public override ConditionResult Check(IDictionary<string, object> env)
        {
            var requestPath = (string)env["owin.RequestPath"];
            var requestQuery = (string)env["owin.RequestQueryString"];

            var pathAndQueryBuilder = new StringBuilder();

            if (requestPath.StartsWith("/"))
            {
                pathAndQueryBuilder.Append(requestPath.Substring(1));
            }

            if (!string.IsNullOrEmpty(requestQuery))
            {
                pathAndQueryBuilder.Append(string.Format("?{0}", requestQuery));
            }

            var match = this.requestPathRegex.Match(pathAndQueryBuilder.ToString());

            if (!match.Success)
            {
                return new ConditionResult(false);
            }

            var successResult = new ConditionResult(true);
            for (var i = 0; i < match.Groups.Count; i++)
            {
                var group = match.Groups[i];
                if (group.Success)
                {
                    var key = string.Format("{{R:{0}}}", i);
                    var value = group.ToString();
                    successResult.CaptureGroups.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            return successResult;
        }
    }
}