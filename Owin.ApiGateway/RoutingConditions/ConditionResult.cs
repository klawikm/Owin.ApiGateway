namespace Owin.ApiGateway.RoutingConditions
{
    using System.Collections.Generic;

    public class ConditionResult
    {
        public ConditionResult(bool success)
        {
            this.Success = success;
            this.CaptureGroups = new List<KeyValuePair<string, string>>();
        }

        public bool Success { get; private set; }

        public IList<KeyValuePair<string, string>> CaptureGroups { get; private set; }
    }
}