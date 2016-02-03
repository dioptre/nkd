using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IGalleryDetailsUriCreator
    {
        string CreateUri(PublishedPackage package);
    }
}