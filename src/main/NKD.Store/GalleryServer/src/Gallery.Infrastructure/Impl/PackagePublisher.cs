using System.Data.Entity;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class PackagePublisher : IPackagePublisher
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IRepository<PublishedScreenshot> _publishedScreenshotRespository;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IPackageLogEntryCreator _packageLogEntryCreator;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;
        private readonly ILatestVersionUpdater<PublishedPackage> _latestVersionUpdater;

        public PackagePublisher(IRepository<Package> packageRepository, IRepository<PublishedPackage> publishedPackageRepository,
            IRepository<PublishedScreenshot> publishedScreenshotRespository, IPackageAuthenticator packageAuthenticator,
            IPackageLogEntryCreator packageLogEntryCreator, IMapper mapper, IDateTime dateTime, ILatestVersionUpdater<PublishedPackage> latestVersionUpdater)
        {
            _packageRepository = packageRepository;
            _publishedPackageRepository = publishedPackageRepository;
            _publishedScreenshotRespository = publishedScreenshotRespository;
            _packageAuthenticator = packageAuthenticator;
            _packageLogEntryCreator = packageLogEntryCreator;
            _mapper = mapper;
            _dateTime = dateTime;
            _latestVersionUpdater = latestVersionUpdater;
        }

        public void PublishPackage(string key, string packageId, string packageVersion, PackageLogAction logActionForExistingPackage)
        {
            var packageLogAction = PackageLogAction.Create;

            _packageAuthenticator.EnsureKeyCanAccessPackage(key, packageId, packageVersion);
            Package package = _packageRepository.Collection
                .Include(p => p.Dependencies)
                .Include(p => p.Screenshots)
                .SingleOrDefault(p => p.Id == packageId && p.Version == packageVersion);
            if (package == null)
            {
                throw new PackageDoesNotExistException(packageId, packageVersion);
            }
            package.Published = _dateTime.UtcNow;
            _packageRepository.Update(package);

            var publishedPackage = _mapper.Map<Package, PublishedPackage>(package);
            var existingPublishedPackageCount =
                _publishedPackageRepository.Collection.Count(pp => pp.Id == packageId && pp.Version == packageVersion);
            if (existingPublishedPackageCount > 0) {
                _publishedScreenshotRespository.DeleteMany(ps => ps.PublishedPackageId == packageId && ps.PublishedPackageVersion == packageVersion);
                _publishedPackageRepository.DeleteMany(pp => pp.Id == packageId && pp.Version == packageVersion);
                packageLogAction = logActionForExistingPackage;
            }
            if (!_publishedPackageRepository.Collection.Any(pp => pp.Id == publishedPackage.Id && pp.IsLatestVersion))
            {
                publishedPackage.IsLatestVersion = true;
            }
            else if (publishedPackage.IsLatestVersion)
            {
                _latestVersionUpdater.SetLatestVersionFlagsOfOtherVersionablesWithSameId(publishedPackage);
            }
            _publishedPackageRepository.Create(publishedPackage);

            _packageLogEntryCreator.Create(packageId, packageVersion, packageLogAction);
        }
    }
}