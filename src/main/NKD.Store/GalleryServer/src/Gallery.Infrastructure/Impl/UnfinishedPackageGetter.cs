using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Extensions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure.Impl
{
    public class UnfinishedPackageGetter : IUnfinishedPackageGetter
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;

        public UnfinishedPackageGetter(IRepository<Package> packageRepository, IPackageAuthenticator packageAuthenticator,
            IRepository<PublishedPackage> publishedPackageRepository)
        {
            _packageRepository = packageRepository;
            _packageAuthenticator = packageAuthenticator;
            _publishedPackageRepository = publishedPackageRepository;
        }

        public IEnumerable<Package> GetUnfinishedPackages(string userKey, IEnumerable<string> userPackageIds)
        {
            if (userPackageIds == null || !userPackageIds.Any())
            {
                return new Package[0];
            }
            _packageAuthenticator.EnsureKeyCanAccessPackages(userPackageIds, userKey);
            IEnumerable<Package> existingPublishedPackages = _publishedPackageRepository.Collection.Where(pp => userPackageIds.Contains(pp.Id)).ToList()
                .Select(pp => new Package { Id = pp.Id, Version = pp.Version});
            return _packageRepository.Collection.Where(p => userPackageIds.Contains(p.Id))
                .Except(existingPublishedPackages, (x, y) => x.Id == y.Id && x.Version == y.Version);
        }
    }
}