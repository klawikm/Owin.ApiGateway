namespace Owin.ApiGateway
{
    using System.Collections.Generic;

    public interface IRoutingService
    {
        string GetRedirectionBaseUri(IDictionary<string, object> env);
    }
}