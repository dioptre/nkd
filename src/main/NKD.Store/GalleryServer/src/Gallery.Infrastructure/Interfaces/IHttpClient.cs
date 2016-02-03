using System;
using Microsoft.Http;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IHttpClient : IDisposable
    {
        HttpResponseMessage Get(string uri);
        HttpResponseMessage Post(string uri, HttpContent body);
    }
}