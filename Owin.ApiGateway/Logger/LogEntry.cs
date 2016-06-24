using System;

namespace Owin.ApiGateway.Logger
{
    public class LogEntry
    {
        public DateTime DateTime { get; set; }

        public string RequestedUrl { get; set; }

        public string SoapAction { get; set; }

        public string RequestString { get; set; }

        public string RequestHeaders { get; set; }

        public byte[] ResponseArray { get; set; }

        public string ResponseHeaders { get; set; }

        public bool IsFromCache { get; set; }

        public int ResponseTimeInMS { get; set; }

        public bool IsResponseGziped { get; set; }

        public bool IsChunkedTransferEncoding { get; set; }
    }
}
