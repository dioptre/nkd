using System;
using System.Configuration;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Core.Extensions;

namespace Gallery.Core.Impl
{
    public class ConfigSettings : IConfigSettings
    {
        private readonly IHttpRuntime _httpRuntime;
        private readonly IConfigurationManager _configurationManager;

        public ConfigSettings(IHttpRuntime httpRuntime, IConfigurationManager configurationManager)
        {
            _httpRuntime = httpRuntime;
            _configurationManager = configurationManager;
        }

        public string TryGetString(string key)
        {
            try
            {
                return _configurationManager.GetString(key);
            }
            catch (MissingAppSettingException)
            {
                return string.Empty;
            }
        }

        public ConnectionStringSettings ConnectionStringSettings(string key)
        {
            return _configurationManager.GetConnectionStringSettings(key);
        }

        public string PhysicalSitePath { get { return _httpRuntime.AppDomainAppPath; } }
        public string RelativePackageDirectory { get { return _configurationManager.GetString("RelativePackageDirectory"); } }
        public string RelativeTemporaryDirectory { get { return _configurationManager.GetString("RelativeTemporaryDirectory"); } }
        public string RelativePluginAssemblyDirectory { get { return _configurationManager.GetString("RelativePluginAssemblyDirectory"); } }
        public string FrontEndWebSiteRoot { get { return _configurationManager.GetString("FrontEndWebSiteRoot"); } }
        public string ValidatePackageKeyUri { get { return _configurationManager.GetString("ValidatePackageKeyUri"); } }
        public string HashAlgorithm { get { return _configurationManager.GetString("HashAlgorithm"); } }
        public bool AuthenticatePackageRequests { get { return _configurationManager.GetBool("AuthenticatePackageRequests"); } }
        public string HashEncodingType { get { return _configurationManager.GetString("HashEncodingType"); } }
        public string MigratorProvider { get { return _configurationManager.GetString("MigratorProvider"); } }
        public string RelativeAssemblyDirectory { get { return _configurationManager.GetString("RelativeAssemblyDirectory"); } }
        public string AuthorizePackageIdsUri { get { return _configurationManager.GetString("AuthorizePackageIdsUri"); } }
        public string AuthorizeRatingsUri { get { return _configurationManager.GetString("AuthorizeRatingsUri"); } }

        public int ExternalPackageRequestTimeout { get { return _configurationManager.GetInt("ExternalPackageRequestTimeout"); } }
        public int MaxPackageLogEntryRecordCount { get { return _configurationManager.GetInt("MaxPackageLogEntryRecordCount"); } }
        public string GalleryDetailsUriTemplate
        {
            get
            {
                return FrontEndWebSiteRoot.AppendWithForwardSlash(_configurationManager.GetString("GalleryDetailsUriTemplate"));
            }
        }
        public string ReportAbuseUriTemplate
        {
            get
            {
                return FrontEndWebSiteRoot.AppendWithForwardSlash(_configurationManager.GetString("ReportAbuseUriTemplate"));
            }
        }
    }
}