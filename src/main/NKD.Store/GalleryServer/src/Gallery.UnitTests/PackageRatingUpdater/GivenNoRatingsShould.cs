using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageRatingUpdater
{
    [TestFixture]
    public class GivenNoRatingsShould
    {
        private Core.Impl.PackageRatingUpdater _packageRatingUpdater;
        private readonly IEnumerable<PackageVersionRatings> _emptyList = new List<PackageVersionRatings>();

        private Mock<IRepository<Package>> _mockedPackageRepository;


        [SetUp]
        public void Establish()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _packageRatingUpdater = new Core.Impl.PackageRatingUpdater(_mockedPackageRepository.Object, new Mock<IPackageDataAggregateUpdater>().Object);
        }

        [Test]
        public void NotThrowAnException()
        {
            TestDelegate methodThatShouldNotThrow = () => _packageRatingUpdater.UpdatePackageRatings(_emptyList);

            Assert.DoesNotThrow(methodThatShouldNotThrow);
        }

        [Test]
        public void NotUpdatePackagesInRepository()
        {
            _packageRatingUpdater.UpdatePackageRatings(_emptyList);

            _mockedPackageRepository.Verify(pr => pr.Update(It.IsAny<IEnumerable<Package>>()), Times.Never());
        }
    }
}