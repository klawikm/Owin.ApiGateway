namespace Owin.ApiGateway.Exceptions
{
    using System;

    [Serializable]
    public class MatchingRoutingConditionNotFoundException : Exception
    {
        public MatchingRoutingConditionNotFoundException(string message)
            : base(message)
        {
        }
    }
}