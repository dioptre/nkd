using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.PackageDataAggregateUpdater
{
    public class IncrementDownloadForPackageTester : PackageDataAggregateUpdaterTester
    {
        private void SetupEmptyPackageDataAggregateCollection()
        {
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new PackageDataAggregate[0].AsQueryable());
            MockedPackageDataAggregateRepository.Setup(pdar => pdar.Create(It.IsAny<PackageDataAggregate>())).Returns(new PackageDataAggregate());
        }

        [Test]
        public void ShouldCreatePackageDataAggregateWhenItDoesNotExist()
        {
            const string nonexistentPackageId = "PackageId";
            SetupEmptyPackageDataAggregateCollection();

            Updater.IncrementDownloadForPackage(nonexistentPackageId);

            MockedPackageDataAggregateRepository.Verify(pdar => pdar.Create(It.IsAny<PackageDataAggregate>()), Times.Once(),
                "The PackageDataAggregate was not created.");
        }

        [Test]
        public void ShouldNotCreateNewAggregateWhenOneWithGivenPackageIdExists()
        {
            var packageDataAggregate = new PackageDataAggregate { PackageId = Guid.NewGuid().ToString() };
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { packageDataAggregate }.AsQueryable());

            Updater.IncrementDownloadForPackage(packageDataAggregate.PackageId);

            MockedPackageDataAggregateRepository.Verify(pdar => pdar.Create(It.IsAny<PackageDataAggregate>()), Times.Never(),
                "Create should not have been invoked.");
        }

        [Test]
        public void ShouldIncrementPackageDownloadCountByOne()
        {
            const int originalDownloadCount = 5;
            var packageDataAggregate = new PackageDataAggregate { PackageId = Guid.NewGuid().ToString(), DownloadCount = originalDownloadCount};
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { packageDataAggregate }.AsQueryable());

            Updater.IncrementDownloadForPackage(packageDataAggregate.PackageId);

            Assert.AreEqual(originalDownloadCount + 1, packageDataAggregate.DownloadCount, "Download count was not incremented.");

        }

        [Test]
        public void ShouldUpdateAggregate()
        {
            string packageId = Guid.NewGuid().ToString();
            var aggregateToUpdate = new PackageDataAggregate {PackageId = packageId};
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { aggregateToUpdate }.AsQueryable());

            Updater.IncrementDownloadForPackage(packageId);

            MockedPackageDataAggregateRepository.Verify(pdar => pdar.Update(aggregateToUpdate), Times.Once(), "Aggregate was not updated.");
        }

        [Test]
        public void ShouldAcquireCollectionOfPublishedPackages()
        {
            SetupEmptyPackageDataAggregateCollection();

            Updater.IncrementDownloadForPackage(Guid.NewGuid().ToString());

            MockedPublishedPackageRepository.VerifyGet(ppr => ppr.Collection, Times.Once(), "Should have acquired collection of PublishedPackages.");
        }

        [Test]
        public void ShouldCallPublishedPackageRepositoryUpdate()
        {
            IQueryable<PublishedPackage> publishedPackages = new[] { new PublishedPackage(), new PublishedPackage()}.AsQueryable();
            SetupEmptyPackageDataAggregateCollection();
            MockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages);

            Updater.IncrementDownloadForPackage(Guid.NewGuid().ToString());

            MockedPublishedPackageRepository.Verify(ppr => ppr.Update(It.IsAny<IEnumerable<PublishedPackage>>()), Times.Once(),
                "PublishedPackageRepository's Update method was not invoked.");
        }

        [Test]
        public void ShouldUpdateDownloadCountOfMatchingPublishedPackage()
        {
            PublishedPackage publishedPackageToUpdate = new PublishedPackage { Id = Guid.NewGuid().ToString(), DownloadCount = 38};
            IQueryable<PublishedPackage> publishedPackages = new[] { publishedPackageToUpdate, new PublishedPackage()}.AsQueryable();
            var aggregate = new PackageDataAggregate { PackageId = publishedPackageToUpdate.Id, DownloadCount = 5 };
            int expectedDownloadCount = aggregate.DownloadCount + 1;
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { aggregate }.AsQueryable());
            MockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages);

            Updater.IncrementDownloadForPackage(publishedPackageToUpdate.Id);

            Assert.AreEqual(expectedDownloadCount, publishedPackageToUpdate.DownloadCount, "DownloadCount for PublishedPackage was not updated.");
        }

        [Test]
        public void ShouldNotUpdateDownloadCountForNonMatchingPublishedPackage()
        {
            const int expectedDownloadCount = 83;
            var publishedPackageToIgnore = new PublishedPackage { DownloadCount = expectedDownloadCount};
            IQueryable<PublishedPackage> publishedPackages = new[] { publishedPackageToIgnore }.AsQueryable();
            MockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages);
            SetupEmptyPackageDataAggregateCollection();

            Updater.IncrementDownloadForPackage(Guid.NewGuid().ToString());

            Assert.AreEqual(expectedDownloadCount, publishedPackageToIgnore.DownloadCount, "DownloadCount for PublishedPackage was not updated.");
        }
    }
}