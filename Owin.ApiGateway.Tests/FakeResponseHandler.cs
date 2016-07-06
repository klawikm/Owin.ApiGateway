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
        private readonly Dictionary<Uri, int> uriCounters = new Dictionary<Uri, int>();

        private readonly Dictionary<Uri, Func<HttpResponseMessage>> _FakeResponsesGeneators = new Dictionary<Uri, Func<HttpResponseMessage>>();

        public void AddFakeResponseGenerator(Uri uri, Func<HttpResponseMessage> responseMessageGenerator)
        {
            _FakeResponsesGeneators.Add(uri, responseMessageGenerator);
        }

        public int GetNumberOfCalls(string uriString)
        {
            var uri = new Uri(uriString);

            if (uriCounters.ContainsKey(uri))
            {
                return uriCounters[uri];
            }

            return 0;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (_FakeResponsesGeneators.ContainsKey(request.RequestUri))
            {
                if (!uriCounters.ContainsKey(request.RequestUri))
                {
                    uriCounters[request.RequestUri] = 0;
                }

                uriCounters[request.RequestUri]++;

                return _FakeResponsesGeneators[request.RequestUri]();
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
            }

        }
    }
}
