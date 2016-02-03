using System;
using System.Net;

namespace Gallery.Server
{
    public class ExtendedWebClient : WebClient
    {
        private readonly int _timeout;

        public ExtendedWebClient(int timeout)
        {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = _timeout;
            }
            return request;
        }
    }
}