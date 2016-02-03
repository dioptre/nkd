using System.Collections.Generic;
using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageDataAggregateUpdater
    {
        void IncrementDownloadForPackage(string packageId);
        void UpdateAggregateRatings(IEnumerable<PackageVersionRatings> packageVersionRatings);
        void RecalculateTotalDownloadCount(string packageId);
    }
}