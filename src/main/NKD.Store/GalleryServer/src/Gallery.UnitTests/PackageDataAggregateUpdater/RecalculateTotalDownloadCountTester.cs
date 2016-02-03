using System.Linq;
using Gallery.Core.Domain;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageDataAggregateUpdater
{
    public class RecalculateTotalDownloadCountTester : PackageDataAggregateUpdaterTester
    {
        private static Package GetPackage(string packageId)
        {
            return new Package {Id = packageId, DownloadCount = 4};
        }

        [Test]
        public void ShouldUpdateDownloadCountOfPackageDataAggregate()
        {
            const string packageId = "PackageId";
            const int expectedDownloadCount = 8;
            PackageDataAggregate aggregateToUpdate = new PackageDataAggregate { PackageId = packageId, DownloadCount = 45 };
            MockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { GetPackage(packageId), GetPackage(packageId) }.AsQueryable());
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] {aggregateToUpdate}.AsQueryable());

            Updater.RecalculateTotalDownloadCount(packageId);

            Assert.AreEqual(expectedDownloadCount, aggregateToUpdate.DownloadCount, "DownloadCount on PackageDataAggregate was not updated.");
        }
    }
}