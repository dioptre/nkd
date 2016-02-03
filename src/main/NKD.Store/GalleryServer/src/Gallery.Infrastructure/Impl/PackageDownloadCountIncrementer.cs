using System;
using System.Linq.Expressions;
using System.Threading;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using System.Linq;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class PackageDownloadCountIncrementer : IPackageDownloadCountIncrementer
    {
        private readonly IRepository<Package> _packageRepostory;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly IPackageDataAggregateUpdater _packageDataAggregateUpdater;
        private readonly IPackageLogEntryCreator _packageLogEntryCreator;
        private readonly IRepository<PackageLogEntry> _packageLogEntryRepository;

        public PackageDownloadCountIncrementer(IRepository<Package> packageRepostory, IRepository<PublishedPackage> publishedPackageRepository,
            IPackageDataAggregateUpdater packageDataAggregateUpdater, IPackageLogEntryCreator packageLogEntryCreator,
            IRepository<PackageLogEntry> packageLogEntryRepository)
        {
            _packageRepostory = packageRepostory;
            _publishedPackageRepository = publishedPackageRepository;
            _packageDataAggregateUpdater = packageDataAggregateUpdater;
            _packageLogEntryCreator = packageLogEntryCreator;
            _packageLogEntryRepository = packageLogEntryRepository;
        }

        public void Increment(string packageId, string packageVersion)
        {
            _packageLogEntryRepository.DeleteMany(ple => ple.PackageId == packageId && ple.PackageVersion == packageVersion
                && ple.Action == PackageLogAction.Download);
            _packageLogEntryCreator.Create(packageId, packageVersion, PackageLogAction.Download);

            Package package = GetPackage(_packageRepostory, p => p.Id == packageId && p.Version == packageVersion);
            PublishedPackage publishedPackage = GetPackage(_publishedPackageRepository, p => p.Id == packageId && p.Version == packageVersion);

            package.DownloadCount++;
            publishedPackage.VersionDownloadCount++;

            _packageDataAggregateUpdater.IncrementDownloadForPackage(packageId);

            _packageRepostory.Update(package);
            _publishedPackageRepository.Update(publishedPackage);
        }

        private static T GetPackage<T>(IRepository<T> repository, Expression<Func<T, bool>> expression)
            where T : class
        {
            T package = repository.Collection.SingleOrDefault(expression);
            if (package == null)
            {
                throw new ObjectDoesNotExistException<T>();
            }
            return package;
        }
    }
}