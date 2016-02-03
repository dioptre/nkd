using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class HttpClientAdapter : IHttpClientAdapter
    {
        public IHttpClient GetHttpClient(string baseAddress)
        {
            return new SystemHttpClient(baseAddress);
        }
    }
}