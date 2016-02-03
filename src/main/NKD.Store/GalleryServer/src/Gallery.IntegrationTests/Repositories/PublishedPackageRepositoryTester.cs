using System;
using Gallery.Core;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using NUnit.Framework;

namespace Gallery.IntegrationTests.Repositories
{
    public class PublishedPackageRepositoryTester : RepositoryTesterBase<PublishedPackage>
    {
        private static readonly string _id = string.Format("Id-{0}", Guid.NewGuid());
        private static readonly string _version = string.Format("Version-{0}", Guid.NewGuid());

        protected override PublishedPackage GetObjectToCreate()
        {
            return new PublishedPackage
            {
                Id = _id,
                Version = _version,
                Created = DateTime.Today,
                LastUpdated = DateTime.Now,
                Published = DateTime.Now,
            };
        }

        protected override Func<PublishedPackage, bool> GetDeletionPredicate()
        {
            return p => p.Id == _id && p.Version == _version;
        }

        private static Package GetValidPackageToSave()
        {
            return new Package
            {
                Id = "Id-" + Guid.NewGuid(),
                Version = "Version",
                Published = DateTime.Now,
                LastUpdated = DateTime.Now,
                Created = DateTime.Now
            };
        }

        [Test]
        public void CanSavePublishedPackageMappedFromPackage()
        {
            Package package = GetValidPackageToSave();
            IMapper mapper = IoC.Resolver.Resolve<IMapper>();
            PublishedPackage publishedPackage = mapper.Map<Package, PublishedPackage>(package);

            Instance.Create(publishedPackage);
        }
    }
}