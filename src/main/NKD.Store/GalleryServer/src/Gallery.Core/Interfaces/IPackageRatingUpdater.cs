using System.Collections.Generic;
using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageRatingUpdater
    {
        void UpdatePackageRatings(IEnumerable<PackageVersionRatings> packageVersionRatings);
    }
}