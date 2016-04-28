namespace Owin.ApiGateway.RoutingConditions
{
    using System.Collections.Generic;

    public class SoapActionCondition : RoutingCondition
    {
        public SoapActionCondition()
        {
        }

        public SoapActionCondition(string requiredSoapAction)
        {
            this.RequiredSoapAction = requiredSoapAction;
        }

        public string RequiredSoapAction { get; set; }

        public override ConditionResult Check(IDictionary<string, object> env)
        {
            string soapAction;

            if (!Tools.TryGetSoapAction(env, out soapAction))
            {
                return new ConditionResult(false);
            }

            var success = soapAction.Equals(this.RequiredSoapAction);

            return new ConditionResult(success);
        }
    }
}