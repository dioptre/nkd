using System;
using System.Linq;
using Gallery.Core.Contants;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class PackageUnpublisher : IPackageUnpublisher
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IRecommendedVersionManager<Package> _packageRecommendedVersionManager;
        private readonly IRecommendedVersionManager<PublishedPackage> _publishedPackageRecommendedVersionManager;
        private readonly IPackageLogEntryCreator _packageLogEntryCreator;

        public PackageUnpublisher(IRepository<Package> packageRepository, IRepository<PublishedPackage> publishedPackageRepository,
            IPackageAuthenticator packageAuthenticator, IRecommendedVersionManager<Package> packageRecommendedVersionManager,
            IRecommendedVersionManager<PublishedPackage> publishedPackageRecommendedVersionManager, IPackageLogEntryCreator packageLogEntryCreator)
        {
            _packageRepository = packageRepository;
            _publishedPackageRepository = publishedPackageRepository;
            _packageAuthenticator = packageAuthenticator;
            _packageRecommendedVersionManager = packageRecommendedVersionManager;
            _publishedPackageRecommendedVersionManager = publishedPackageRecommendedVersionManager;
            _packageLogEntryCreator = packageLogEntryCreator;
        }

        public void UnpublishPackage(string key, string packageId, string packageVersion)
        {
            _packageAuthenticator.EnsureKeyCanAccessPackage(key, packageId, packageVersion);
            Package package = _packageRepository.Collection.SingleOrDefault(p => p.Id == packageId && p.Version == packageVersion);
            if (package == null)
            {
                throw new PackageDoesNotExistException(packageId, packageVersion);
            }
            PublishedPackage publishedPackage = _publishedPackageRepository.Collection.SingleOrDefault(p => p.Id == packageId && p.Version == packageVersion);
            VerifyPackageIsInPublishedState(package, publishedPackage);
            Unpublish(package, publishedPackage);

            if (package.IsLatestVersion)
            {
                _packageRecommendedVersionManager.SetLatestVersionAsRecommended(packageId, false);
                _publishedPackageRecommendedVersionManager.SetLatestVersionAsRecommended(packageId, true);
                UnsetRecommendedVersion(package, publishedPackage);
            }

            _packageLogEntryCreator.Create(packageId, packageVersion, PackageLogAction.Unpublish);
        }

        private void UnsetRecommendedVersion(Package package, PublishedPackage publishedPackage)
        {
            package.IsLatestVersion = false;
            publishedPackage.IsLatestVersion = false;
            _packageRepository.Update(package);
            _publishedPackageRepository.Update(publishedPackage);
        }

        private void Unpublish(Package package, PublishedPackage publishedPackage)
        {
            package.Published = Constants.UnpublishedDate;
            publishedPackage.Published = Constants.UnpublishedDate;
            _packageRepository.Update(package);
            _publishedPackageRepository.Update(publishedPackage);
        }

        private static void VerifyPackageIsInPublishedState(Package package, PublishedPackage publishedPackage)
        {
            if (package.Published == Constants.UnpublishedDate || publishedPackage == null || publishedPackage.Published == Constants.UnpublishedDate)
            {
                throw new InvalidOperationException("Cannot Unpublish a Package that is not currently Published.");
            }
        }
    }
}