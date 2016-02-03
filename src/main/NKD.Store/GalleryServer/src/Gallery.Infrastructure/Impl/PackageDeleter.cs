using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class PackageDeleter : IPackageDeleter
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Screenshot> _screenshotRespository;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IRepository<PublishedScreenshot> _publishedScreenshotRepository;
        private readonly IPackageFileGetter _packageFileGetter;
        private readonly IFileSystem _fileSystem;
        private readonly IPackageLogEntryCreator _packageLogEntryCreator;
        private readonly IRepository<Dependency> _dependencyRepository;
        private readonly IRecommendedVersionManager<Package> _packageRecommendedVersionManager;
        private readonly IRecommendedVersionManager<PublishedPackage> _publishedPackagePublishedPackageRecommendedVersionManager;
        private readonly IPackageDataAggregateUpdater _packageDataAggregateUpdater;
        private readonly IRepository<PackageDataAggregate> _packageDataAggregateRepository;

        public PackageDeleter(IRepository<Package> packageRepository, IRepository<Screenshot> screenshotRespository,
            IRepository<PublishedPackage> publishedPackageRepository, IRepository<PublishedScreenshot> publishedScreenshotRepository,
            IPackageFileGetter packageFileGetter, IFileSystem fileSystem, IPackageLogEntryCreator packageLogEntryCreator,
            IRepository<Dependency> dependencyRepository, IRecommendedVersionManager<Package> packageRecommendedVersionManager,
            IRecommendedVersionManager<PublishedPackage> publishedPackageRecommendedVersionManager, IPackageDataAggregateUpdater packageDataAggregateUpdater, IRepository<PackageDataAggregate> packageDataAggregateRepository)
        {
            _packageLogEntryCreator = packageLogEntryCreator;
            _packageDataAggregateRepository = packageDataAggregateRepository;
            _dependencyRepository = dependencyRepository;
            _packageRepository = packageRepository;
            _screenshotRespository = screenshotRespository;
            _publishedPackageRepository = publishedPackageRepository;
            _publishedScreenshotRepository = publishedScreenshotRepository;
            _packageFileGetter = packageFileGetter;
            _fileSystem = fileSystem;
            _packageRecommendedVersionManager = packageRecommendedVersionManager;
            _publishedPackagePublishedPackageRecommendedVersionManager = publishedPackageRecommendedVersionManager;
            _packageDataAggregateUpdater = packageDataAggregateUpdater;
        }

        public void DeletePackage(string packageId, string packageVersion)
        {
            Package existingPackage = _packageRepository.Collection.SingleOrDefault(p => p.Id == packageId && p.Version == packageVersion);
            if (existingPackage == null)
            {
                throw new PackageDoesNotExistException(packageId, packageVersion);
            }
            bool wasPackageRecommended = existingPackage.IsLatestVersion;
            _screenshotRespository.DeleteMany(s => s.PackageId == packageId && s.PackageVersion == packageVersion);
            _dependencyRepository.DeleteMany(d => d.PackageId == packageId && d.PackageVersion == packageVersion);
            _packageRepository.DeleteSingle(p => p.Id == packageId && p.Version == packageVersion);

            _publishedScreenshotRepository.DeleteMany(ps => ps.PublishedPackageId == packageId && ps.PublishedPackageVersion == packageVersion);
            _publishedPackageRepository.DeleteMany(p => p.Id == packageId && p.Version == packageVersion);

            _packageLogEntryCreator.Create(packageId, packageVersion, PackageLogAction.Delete);

            var filePath = _packageFileGetter.GetPackagePath(packageId, packageVersion);
            _fileSystem.DeleteFileIfItExists(filePath);
            _fileSystem.DeleteDirectoryIfEmpty(_packageFileGetter.GetPackageDirectory(packageId, packageVersion), true);

            if (wasPackageRecommended)
            {
                _packageRecommendedVersionManager.SetLatestVersionAsRecommended(packageId, false);
                _publishedPackagePublishedPackageRecommendedVersionManager.SetLatestVersionAsRecommended(packageId, true);
            }

            if (_packageRepository.Collection.Any(p => p.Id == packageId))
            {
                _packageDataAggregateUpdater.RecalculateTotalDownloadCount(packageId);
                _packageDataAggregateUpdater.UpdateAggregateRatings(new[] {new PackageVersionRatings {PackageId = packageId, PackageVersion = packageVersion}});
            }
            else
            {
                _packageDataAggregateRepository.DeleteSingle(pda => pda.PackageId == packageId);
            }
        }
    }
}