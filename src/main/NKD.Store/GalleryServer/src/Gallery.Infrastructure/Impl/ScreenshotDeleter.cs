using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class ScreenshotDeleter : IScreenshotDeleter
    {
        private readonly IRepository<Screenshot> _screenshotRepository;
        private readonly IRepository<PublishedScreenshot> _publishedScreenshotRepository;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IMapper _mapper;

        public ScreenshotDeleter(IRepository<Screenshot> screenshotRepository, IRepository<PublishedScreenshot> publishedScreenshotRepository, IMapper mapper,
            IRepository<PublishedPackage> publishedPackageRepository)
        {
            _screenshotRepository = screenshotRepository;
            _publishedPackageRepository = publishedPackageRepository;
            _publishedScreenshotRepository = publishedScreenshotRepository;
            _mapper = mapper;
        }

        public void DeleteScreenshot(int screenshotId)
        {
            Screenshot screenshot = _screenshotRepository.Collection.Single(s => s.Id == screenshotId);
            string packageId = screenshot.PackageId;
            string packageVersion = screenshot.PackageVersion;

            _screenshotRepository.DeleteSingle(s => s.Id == screenshotId);

            _publishedScreenshotRepository.DeleteMany(ps => ps.PublishedPackageId == packageId && ps.PublishedPackageVersion == packageVersion);
            if (_publishedPackageRepository.Collection.Any(pp => pp.Id == screenshot.PackageId && pp.Version == screenshot.PackageVersion))
            {
                IQueryable<Screenshot> screenshotsToPublish = _screenshotRepository.Collection
                    .Where(s => s.PackageId == packageId && s.PackageVersion == packageVersion);
                foreach (var screenshotToPublish in screenshotsToPublish)
                {
                    _publishedScreenshotRepository.Create(_mapper.Map<Screenshot, PublishedScreenshot>(screenshotToPublish));
                }
            }
        }
    }
}