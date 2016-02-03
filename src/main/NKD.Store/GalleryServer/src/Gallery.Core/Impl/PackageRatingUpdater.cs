using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageRatingUpdater : IPackageRatingUpdater
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IPackageDataAggregateUpdater _packageDataAggregateUpdater;

        public PackageRatingUpdater(IRepository<Package> packageRepository, IPackageDataAggregateUpdater packageDataAggregateUpdater)
        {
            _packageRepository = packageRepository;
            _packageDataAggregateUpdater = packageDataAggregateUpdater;
        }

        public void UpdatePackageRatings(IEnumerable<PackageVersionRatings> packageVersionRatings)
        {
            UpdatePackages(packageVersionRatings);
            _packageDataAggregateUpdater.UpdateAggregateRatings(packageVersionRatings);
        }

        private void UpdatePackages(IEnumerable<PackageVersionRatings> packageVersionRatings)
        {
            var packagesToUpdate = new List<Package>();
            foreach (var versionRatings in packageVersionRatings)
            {
                PackageVersionRatings ratingsClosure = versionRatings;
                Package package = _packageRepository.Collection.Single(p => p.Id == ratingsClosure.PackageId && p.Version == ratingsClosure.PackageVersion);
                package.RatingAverage = ratingsClosure.RatingAverage;
                package.RatingsCount = ratingsClosure.RatingCount;
                packagesToUpdate.Add(package);
            }
            if (packagesToUpdate.Any())
            {
                _packageRepository.Update(packagesToUpdate);
            }
        }
    }
}