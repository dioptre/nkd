using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.PackageRatingUpdater
{
    [TestFixture]
    public class GivenPackageVersionRatingsShould
    {
        private Core.Impl.PackageRatingUpdater _packageRatingUpdater;
        private IQueryable<Package> _packagesFromRepository;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IPackageDataAggregateUpdater> _mockedAggregateUpdater;
        private PackageVersionRatings[] _packageVersionRatings;

        [SetUp]
        public void Establish()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedAggregateUpdater = new Mock<IPackageDataAggregateUpdater>();

            _packagesFromRepository = new[]
            {
                new Package {Id = "pack1", Version = "1.0", RatingAverage = 3.2, RatingsCount = 8},
                new Package {Id = "pack3", Version = "3.1", RatingAverage = 4.5, RatingsCount = 15},
                new Package {Id = "pack2", Version = "2.0", RatingAverage = 3.0, RatingsCount = 45},
                new Package {Id = "pack1", Version = "1.1", RatingAverage = 3.4, RatingsCount = 12},
                new Package {Id = "pack4", Version = "4.2.1", RatingAverage = 4.0, RatingsCount = 27},
            }.AsQueryable();
            _mockedPackageRepository.SetupGet(r => r.Collection).Returns(_packagesFromRepository);

            _packageRatingUpdater = new Core.Impl.PackageRatingUpdater(_mockedPackageRepository.Object, _mockedAggregateUpdater.Object);
            _packageVersionRatings = new[]
            {
                new PackageVersionRatings { PackageId = "pack1", PackageVersion = "1.0", RatingAverage = 3.5, RatingCount =  10},
                new PackageVersionRatings { PackageId = "pack2", PackageVersion = "2.0", RatingAverage = 2.8, RatingCount =  50}
            };
            _packageRatingUpdater.UpdatePackageRatings(_packageVersionRatings);
        }

        [Test]
        public void UpdateRatingAggregateValuesOnCorrespondingPackages()
        {
            Assert.AreEqual(3.5, _packagesFromRepository.Single(p => p.Id == "pack1" && p.Version == "1.0").RatingAverage);
            Assert.AreEqual(10, _packagesFromRepository.Single(p => p.Id == "pack1" && p.Version == "1.0").RatingsCount);
            Assert.AreEqual(2.8, _packagesFromRepository.Single(p => p.Id == "pack2").RatingAverage);
            Assert.AreEqual(50, _packagesFromRepository.Single(p => p.Id == "pack2").RatingsCount);

            Assert.AreEqual(4.5, _packagesFromRepository.Single(p => p.Id == "pack3").RatingAverage);
            Assert.AreEqual(15, _packagesFromRepository.Single(p => p.Id == "pack3").RatingsCount);
            Assert.AreEqual(4.0, _packagesFromRepository.Single(p => p.Id == "pack4").RatingAverage);
            Assert.AreEqual(27, _packagesFromRepository.Single(p => p.Id == "pack4").RatingsCount);
            Assert.AreEqual(3.4, _packagesFromRepository.Single(p => p.Id == "pack1" && p.Version == "1.1").RatingAverage);
            Assert.AreEqual(12, _packagesFromRepository.Single(p => p.Id == "pack1" && p.Version == "1.1").RatingsCount);
        }

        [Test]
        public void UpdatePackagesInRepository()
        {
            _mockedPackageRepository.Verify(pr => pr.Update(It.IsAny<IEnumerable<Package>>()), Times.Once());
        }

        [Test]
        public void CallAggregateUpdater()
        {
            _mockedAggregateUpdater.Verify(au => au.UpdateAggregateRatings(_packageVersionRatings), Times.Once());
        }
    }
}