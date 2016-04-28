namespace Owin.ApiGateway.RoutingConditions
{
    using System.Collections.Generic;

    public class AlwaysMatchingCondition : RoutingCondition
    {
        public override ConditionResult Check(IDictionary<string, object> env)
        {
            return new ConditionResult(true);
        }
    }
}