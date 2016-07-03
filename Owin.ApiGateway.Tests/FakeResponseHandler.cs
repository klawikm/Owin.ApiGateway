using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Owin.ApiGateway.Tests
{
    public class FakeResponseHandler : DelegatingHandler
    {
        private readonly Dictionary<Uri, Func<HttpResponseMessage>> _FakeResponsesGeneators = new Dictionary<Uri, Func<HttpResponseMessage>>();

        public void AddFakeResponseGenerator(Uri uri, Func<HttpResponseMessage> responseMessageGenerator)
        {
            _FakeResponsesGeneators.Add(uri, responseMessageGenerator);
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (_FakeResponsesGeneators.ContainsKey(request.RequestUri))
            {
                return _FakeResponsesGeneators[request.RequestUri]();
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
            }

        }
    }
}
