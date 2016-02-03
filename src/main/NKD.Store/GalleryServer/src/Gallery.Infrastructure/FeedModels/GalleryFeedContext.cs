using System.Data.Entity;
using System.Linq;
using Gallery.Core;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.FeedModels
{
    public class GalleryFeedContext
    {
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IRepository<PublishedScreenshot> _publishedScreenshotRepository;

        public GalleryFeedContext()
            : this(IoC.Resolver.Resolve<IRepository<PublishedPackage>>(), IoC.Resolver.Resolve<IRepository<PublishedScreenshot>>())
        { }

        public GalleryFeedContext(IRepository<PublishedPackage> publishedPackageRepository, IRepository<PublishedScreenshot> publishedScreenshotRepository)
        {
            _publishedPackageRepository = publishedPackageRepository;
            _publishedScreenshotRepository = publishedScreenshotRepository;
        }

        public IQueryable<PublishedPackage> Packages { get { return _publishedPackageRepository.Collection.Include(p => p.Screenshots); } }
        public IQueryable<PublishedScreenshot> Screenshots { get { return _publishedScreenshotRepository.Collection; } }
    }
}