using System;
using System.Net;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Microsoft.Http;
using Ninject.Extensions.Logging;

namespace Gallery.Infrastructure.Impl
{
    public class RatingAuthorizer : IRatingAuthorizer
    {
        private readonly IHttpClientAdapter _httpClientAdapter;
        private readonly IConfigSettings _configSettings;
        private readonly ILogger _logger;

        public RatingAuthorizer(IHttpClientAdapter httpClientAdapter, IConfigSettings configSettings, ILogger logger)
        {
            _httpClientAdapter = httpClientAdapter;
            _configSettings = configSettings;
            _logger = logger;
        }

        public void ValidateNonce(string nonce)
        {
            using (IHttpClient client = _httpClientAdapter.GetHttpClient(_configSettings.FrontEndWebSiteRoot))
            {
                string uri = string.Format("{0}/{1}", _configSettings.AuthorizeRatingsUri, nonce);
                using (HttpResponseMessage response = client.Get(uri))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Error("External call returned non-OK status of '{0}'. Authorization for Ratings Update failed.", response.StatusCode);
                        throw new Exception("An error occurred when trying to authorize access to Ratings Update.");
                    }
                    bool accessAllowed;
                    if (!bool.TryParse(response.Content.ReadAsString(), out accessAllowed) || !accessAllowed)
                    {
                        _logger.Error("The given key was refused access for updating the aggregate ratings.");
                        throw new Exception("The given key was refused access for updating the aggregate ratings.");
                    }
                }
            }
        }
    }
}