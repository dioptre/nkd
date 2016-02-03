using System;
using System.Configuration;
using System.IO;
using Gallery.Core.Interfaces;

namespace Gallery.IntegrationTests.Helpers
{
    internal class IntegrationTestConfigSettings : IConfigSettings
    {
        public string TryGetString(string key) { throw new NotImplementedException(); }
        public string PhysicalSitePath { get { return Directory.GetCurrentDirectory(); } }
        public string RelativePackageDirectory { get { throw new NotImplementedException(); } }
        public string RelativeTemporaryDirectory { get { throw new NotImplementedException(); } }
        public string RelativePluginAssemblyDirectory { get { throw new NotImplementedException(); } }
        public string FrontEndWebSiteRoot { get { throw new NotImplementedException(); } }
        public string ValidatePackageKeyUri { get { throw new NotImplementedException(); } }
        public string GalleryDetailsUriTemplate { get { throw new NotImplementedException(); } }
        public string ReportAbuseUriTemplate { get { throw new NotImplementedException(); } }
        public string HashAlgorithm { get { throw new NotImplementedException(); } }
        public bool AuthenticatePackageRequests { get { throw new NotImplementedException(); } }
        public string HashEncodingType { get { throw new NotImplementedException(); } }
        public string MigratorProvider { get { return "SqlServerCe"; } }
        public string RelativeAssemblyDirectory { get { return string.Empty; } }
        public string AuthorizePackageIdsUri { get { throw new NotImplementedException(); } }
        public string AuthorizeRatingsUri { get { throw new NotImplementedException(); } }

        public int ExternalPackageRequestTimeout { get { throw new NotImplementedException(); } }
        public int MaxPackageLogEntryRecordCount { get { throw new NotImplementedException(); } }

        public ConnectionStringSettings ConnectionStringSettings(string key) { return ConfigurationManager.ConnectionStrings[key]; }
    }
}