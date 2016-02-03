using System.Data.Entity;
using Gallery.Core.Domain;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class GalleryFeedEntities : DbContext, IDatabaseContext
    {
        public IDbSet<Dependency> Dependencies { get; set; }
        public IDbSet<Package> Packages { get; set; }
        public IDbSet<Screenshot> Screenshots { get; set; }
        public IDbSet<PackageLogEntry> PackageLogEntries { get; set; }
        public IDbSet<PackageDataAggregate> PackageDataAggregates { get; set; }

        public IDbSet<PublishedPackage> PublishedPackages { get; set; }
        public IDbSet<PublishedScreenshot> PublishedScreenshots { get; set; }

        public GalleryFeedEntities()
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>().HasKey(p => new { p.Id, p.Version }).ToTable("Package");
            modelBuilder.Entity<PublishedPackage>().HasKey(pp => new { pp.Id, pp.Version }).ToTable("PublishedPackage");

            modelBuilder.Entity<PackageDataAggregate>().ToTable("PackageDataAggregate");
            modelBuilder.Entity<Dependency>().ToTable("Dependency");
            modelBuilder.Entity<Screenshot>().ToTable("Screenshot");
            modelBuilder.Entity<PublishedScreenshot>().ToTable("PublishedScreenshot");
            modelBuilder.Entity<PackageLogEntry>().ToTable("PackageLogEntry");
            modelBuilder.Entity<PackageLogEntry>().Ignore(ple => ple.Action);

            base.OnModelCreating(modelBuilder);
        }
    }
}