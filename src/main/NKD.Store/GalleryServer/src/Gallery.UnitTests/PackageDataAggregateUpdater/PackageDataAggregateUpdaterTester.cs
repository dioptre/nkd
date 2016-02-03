using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageDataAggregateUpdater
{
    [TestFixture]
    public abstract class PackageDataAggregateUpdaterTester
    {
        protected Infrastructure.Impl.PackageDataAggregateUpdater Updater;

        protected Mock<IRepository<PackageDataAggregate>> MockedPackageDataAggregateRepository;
        protected Mock<IRepository<PublishedPackage>> MockedPublishedPackageRepository;
        protected Mock<IRepository<Package>> MockedPackageRepository;

        [SetUp]
        public void SetUp()
        {
            MockedPackageDataAggregateRepository = new Mock<IRepository<PackageDataAggregate>>();
            MockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            MockedPackageRepository = new Mock<IRepository<Package>>();

            Updater = new Infrastructure.Impl.PackageDataAggregateUpdater(MockedPackageDataAggregateRepository.Object,
                MockedPublishedPackageRepository.Object, MockedPackageRepository.Object);
        }
    }
}