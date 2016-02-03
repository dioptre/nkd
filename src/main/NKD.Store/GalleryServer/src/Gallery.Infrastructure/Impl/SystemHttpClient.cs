using Gallery.Infrastructure.Interfaces;
using Microsoft.Http;

namespace Gallery.Infrastructure.Impl
{
    public class SystemHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public SystemHttpClient(string baseAddress)
        {
            _httpClient = new HttpClient(baseAddress);
        }

        public HttpResponseMessage Get(string uri)
        {
            return _httpClient.Get(uri);
        }

        public HttpResponseMessage Post(string uri, HttpContent body)
        {
            return _httpClient.Post(uri, body);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}