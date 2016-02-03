using System;
using System.Data.Entity.Infrastructure;
using Gallery.Core.Domain;
using NUnit.Framework;

namespace Gallery.IntegrationTests.Repositories
{
    public class PackageDataAggregateRepositoryTester : RepositoryTesterBase<PackageDataAggregate>
    {
        private readonly string _packageIdForAggregate = "IntegrationTest" + Guid.NewGuid();

        protected override PackageDataAggregate GetObjectToCreate()
        {
            return new PackageDataAggregate { PackageId = _packageIdForAggregate};
        }

        protected override Func<PackageDataAggregate, bool> GetDeletionPredicate()
        {
            return pda => pda.PackageId == _packageIdForAggregate;
        }

        [Test]
        public void InsertingSecondAggregateWithDuplicatePackageIdShouldFail()
        {
            var aggregateInStorage = GetObjectToCreate();
            var aggregateWithDuplicatePackageId = GetObjectToCreate();

            Instance.Create(aggregateInStorage);
            TestDelegate methodThatShouldThrow = () => Instance.Create(aggregateWithDuplicatePackageId);

            Assert.Throws<DbUpdateException>(methodThatShouldThrow, "Second Create call should have thrown an exception.");
        }
    }
}