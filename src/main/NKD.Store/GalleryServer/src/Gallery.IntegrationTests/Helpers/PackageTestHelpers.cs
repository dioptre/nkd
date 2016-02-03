using System;
using System.Linq;
using Gallery.Core;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.IntegrationTests.Helpers
{
    public static class PackageTestHelpers
    {
        public static Package GetExistingPackage()
        {
            var packageRepository = IoC.Resolver.Resolve<IRepository<Package>>();
            return packageRepository.Collection.FirstOrDefault() ??
                   packageRepository.Create(new Package { Id = Guid.NewGuid().ToString(), Version  = "1.0.5.3",
                    Created = DateTime.Now, LastUpdated = DateTime.Today });
        }

        public static PublishedPackage GetExistingPublishedPackage()
        {
            var publishedPackageRepository = IoC.Resolver.Resolve<IRepository<PublishedPackage>>();
            return publishedPackageRepository.Collection.FirstOrDefault() ?? publishedPackageRepository.Create(new PublishedPackage
            {
                Id = Guid.NewGuid().ToString(),
                Version = Guid.NewGuid().ToString(),
                Created = DateTime.Today,
                LastUpdated = DateTime.UtcNow,
                Published = DateTime.Now,
            });
        }
    }
}