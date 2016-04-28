namespace FakeHttpService
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Tools
    {
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
    }
}