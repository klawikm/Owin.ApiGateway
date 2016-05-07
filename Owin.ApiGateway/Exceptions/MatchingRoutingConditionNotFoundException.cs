namespace Owin.ApiGateway.Exceptions
{
    using System;

    public class MatchingRoutingConditionNotFoundException : Exception
    {
        public MatchingRoutingConditionNotFoundException(string message)
            : base(message)
        {
        }
    }
}