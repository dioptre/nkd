using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IPackageReportAbuseUriCreator
    {
        string CreateUri(PublishedPackage package);
    }
}