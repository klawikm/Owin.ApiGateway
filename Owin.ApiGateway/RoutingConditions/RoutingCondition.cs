namespace Owin.ApiGateway.RoutingConditions
{
    using System;
    using System.Collections.Generic;

    public class RoutingCondition
    {
        public virtual ConditionResult Check(IDictionary<string, object> env)
        {
            throw new NotImplementedException();
        }
    }
}