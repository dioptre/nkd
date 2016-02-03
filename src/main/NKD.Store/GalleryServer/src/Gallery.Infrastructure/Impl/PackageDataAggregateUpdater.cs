using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class PackageDataAggregateUpdater : IPackageDataAggregateUpdater
    {
        private readonly IRepository<PackageDataAggregate> _packageDataAggregateRepository;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IRepository<Package> _packageRepository;

        public PackageDataAggregateUpdater(IRepository<PackageDataAggregate> packageDataAggregateRepository,
            IRepository<PublishedPackage> publishedPackageRepository, IRepository<Package> packageRepository)
        {
            _packageDataAggregateRepository = packageDataAggregateRepository;
            _packageRepository = packageRepository;
            _publishedPackageRepository = publishedPackageRepository;
        }

        public void IncrementDownloadForPackage(string packageId)
        {
            PackageDataAggregate packageDataAggregate = GetAggregate(packageId);
            packageDataAggregate.DownloadCount++;

            _packageDataAggregateRepository.Update(packageDataAggregate);
            UpdatePublishedPackageDownloadCounts(packageId, packageDataAggregate.DownloadCount);
        }

        public void UpdateAggregateRatings(IEnumerable<PackageVersionRatings> packageVersionRatings)
        {
            var updatedAggregates = new List<PackageDataAggregate>();
            foreach (var packageId in packageVersionRatings.Select(pvr => pvr.PackageId).Distinct())
            {
                updatedAggregates.Add(UpdateAggregateRatingsForPackage(packageId));
            }
            UpdatePublishedPackageRatings(packageVersionRatings, updatedAggregates);
        }

        public void RecalculateTotalDownloadCount(string packageId)
        {
            PackageDataAggregate packageDataAggregate = GetAggregate(packageId);
            packageDataAggregate.DownloadCount = _packageRepository.Collection.Where(p => p.Id == packageId).Sum(p => p.DownloadCount);
            _packageDataAggregateRepository.Update(packageDataAggregate);
            UpdatePublishedPackageDownloadCounts(packageId, packageDataAggregate.DownloadCount);
        }

        private void UpdatePublishedPackageRatings(IEnumerable<PackageVersionRatings> packageVersionRatings,
            IEnumerable<PackageDataAggregate> packageDataAggregates)
        {
            foreach (var versionRatings in packageVersionRatings)
            {
                PackageVersionRatings ratingClosure = versionRatings;
                var aggregate = packageDataAggregates.Single(pda => pda.PackageId == ratingClosure.PackageId);
                var publishedPackagesToUpdate = _publishedPackageRepository.Collection.Where(pp => pp.Id == ratingClosure.PackageId).ToList();
                foreach (var publishedPackage in publishedPackagesToUpdate)
                {
                    PublishedPackage package = publishedPackage;
                    if (versionRatings.PackageId == publishedPackage.Id && versionRatings.PackageVersion == publishedPackage.Version)
                    {
                        PackageVersionRatings ratingsToUse = packageVersionRatings.Single(vr => vr.PackageId == package.Id
                            && vr.PackageVersion == package.Version);
                        package.VersionRating = ratingsToUse.RatingAverage;
                        package.VersionRatingsCount = ratingsToUse.RatingCount;
                    }
                    package.RatingsCount = aggregate.RatingsCount;
                    package.Rating = aggregate.Rating;
                }
                _publishedPackageRepository.Update(publishedPackagesToUpdate);
            }
        }

        private PackageDataAggregate UpdateAggregateRatingsForPackage(string packageId)
        {
            PackageDataAggregate packageDataAggregate = GetAggregate(packageId);
            IQueryable<Package> packages = _packageRepository.Collection.Where(p => p.Id == packageId);

            int totalRatingsCount = packages.Sum(p => p.RatingsCount);
            packageDataAggregate.RatingsCount = totalRatingsCount;
            if (totalRatingsCount != 0)
            {
                double sumOfAllPackageRatings = packages.Sum(p => p.RatingAverage * p.RatingsCount);
                packageDataAggregate.Rating = sumOfAllPackageRatings / totalRatingsCount;
            }
            else
            {
                packageDataAggregate.Rating = 0;
            }
            _packageDataAggregateRepository.Update(packageDataAggregate);
            return packageDataAggregate;
        }

        private PackageDataAggregate GetAggregate(string packageId)
        {
            return _packageDataAggregateRepository.Collection.SingleOrDefault(pda => pda.PackageId == packageId)
                ?? _packageDataAggregateRepository.Create(new PackageDataAggregate { PackageId = packageId });
        }

        private void UpdatePublishedPackageDownloadCounts(string packageId, int newDownloadCount)
        {
            var publishedPackagesToUpdate = _publishedPackageRepository.Collection.Where(pp => pp.Id == packageId).ToList();
            foreach (var publishedPackage in publishedPackagesToUpdate)
            {
                publishedPackage.DownloadCount = newDownloadCount;
            }
            _publishedPackageRepository.Update(publishedPackagesToUpdate);
        }
    }
}