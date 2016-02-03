using System.Configuration;

namespace Gallery.Core.Interfaces
{
    public interface IConfigSettings
    {
        string TryGetString(string key);
        ConnectionStringSettings ConnectionStringSettings(string key);

        string PhysicalSitePath { get; }
        string RelativePackageDirectory { get; }
        string RelativeTemporaryDirectory { get; }
        string RelativePluginAssemblyDirectory { get; }
        string FrontEndWebSiteRoot { get; }
        string ValidatePackageKeyUri { get; }
        string GalleryDetailsUriTemplate { get; }
        string ReportAbuseUriTemplate { get; }
        string HashAlgorithm { get; }
        bool AuthenticatePackageRequests { get; }
        string HashEncodingType { get; }
        string MigratorProvider { get; }
        string RelativeAssemblyDirectory { get; }
        string AuthorizePackageIdsUri { get; }
        string AuthorizeRatingsUri { get; }
        int ExternalPackageRequestTimeout { get; }
        int MaxPackageLogEntryRecordCount { get; }
    }
}