using System.Data.Entity;
using Gallery.Core.Domain;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IDatabaseContext
    {
        IDbSet<Dependency> Dependencies { get; set; }
        IDbSet<Package> Packages { get; set; }
        IDbSet<Screenshot> Screenshots { get; set; }
        IDbSet<PackageLogEntry> PackageLogEntries { get; set; }
        IDbSet<PackageDataAggregate> PackageDataAggregates { get; set; }

        IDbSet<PublishedPackage> PublishedPackages { get; set; }
        IDbSet<PublishedScreenshot> PublishedScreenshots { get; set; }

        int SaveChanges();
    }
}