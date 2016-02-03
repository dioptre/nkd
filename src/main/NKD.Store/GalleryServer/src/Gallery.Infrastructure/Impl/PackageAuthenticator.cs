using System;
using System.Collections.Generic;
using System.Net;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Microsoft.Http;
using Ninject.Extensions.Logging;

namespace Gallery.Infrastructure.Impl
{
    public class PackageAuthenticator : IPackageAuthenticator
    {
        private readonly IHttpClientAdapter _httpClientAdapter;
        private readonly IConfigSettings _configSettings;
        private readonly ILogger _logger;

        public PackageAuthenticator(IHttpClientAdapter httpClientAdapter, IConfigSettings configSettings, ILogger logger)
        {
            _httpClientAdapter = httpClientAdapter;
            _configSettings = configSettings;
            _logger = logger;
        }

        public void EnsureKeyCanAccessPackage(string key, string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(packageId) || string.IsNullOrWhiteSpace(packageVersion))
            {
                _logger.Error("Invalid key/packageId/packageVersion passed to Package Authorization method. No external authorization was performed.");
                throw new PackageAuthorizationException();
            }
            string uri = string.Format("{0}/{1}/{2}/{3}", _configSettings.ValidatePackageKeyUri, key, packageId, packageVersion);
            Verify(uri, packageId);
        }

        public void EnsureKeyCanAccessPackages(IEnumerable<string> packageIds, string key)
        {
            using (IHttpClient client = _httpClientAdapter.GetHttpClient(_configSettings.FrontEndWebSiteRoot))
            {
                string uri = string.Format("{0}/{1}", _configSettings.AuthorizePackageIdsUri, key);
                HttpContent content = HttpContentExtensions.CreateJsonDataContract(packageIds);
                using (HttpResponseMessage response = client.Post(uri, content))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Error("External call returned non-OK status of '{0}'. Authorization for Packages failed.", response.StatusCode);
                        throw new Exception("An error occurred when trying to authorize access to PackagesIds.");
                    }
                    bool accessAllowed;
                    if (!bool.TryParse(response.Content.ReadAsString(), out accessAllowed) || !accessAllowed)
                    {
                        _logger.Error("The given userkey was refused access one or more of the given Package IDs.");
                        throw new PackageAuthorizationException();
                    }
                }
            }
        }

        private void Verify(string uri, string packageId)
        {
            using (IHttpClient client = _httpClientAdapter.GetHttpClient(_configSettings.FrontEndWebSiteRoot))
            {
                using (HttpResponseMessage response = client.Get(uri))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.Error("External call returned non-OK status of '{0}'. Package Authorization failed.", response.StatusCode);
                        throw new Exception(string.Format("An error occurred when trying to authenticate access to Packages with ID '{0}'.", packageId));
                    }
                    bool accessAllowed;
                    if (!bool.TryParse(response.Content.ReadAsString(), out accessAllowed) || !accessAllowed)
                    {
                        _logger.Error("The given userkey was refused access to the PackageID '{0}'.", packageId);
                        throw new PackageAuthorizationException(packageId);
                    }
                }
            }
        }
    }
}