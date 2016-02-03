using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.PackageDownloadCountIncrementer
{
    [TestFixture]
    public class IncrementTester
    {
        private IPackageDownloadCountIncrementer _packageDownloadCountIncrementer;

        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;
        private Mock<IPackageDataAggregateUpdater> _mockedPackageDataAggregateUpdater;

        private readonly Package _existingPackage = new Package {Id = "Existing", Version = "1.5"};
        private readonly PublishedPackage _existingPublishedPackage = new PublishedPackage { Id = "Existing", Version = "1.5" };

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            _mockedPackageDataAggregateUpdater = new Mock<IPackageDataAggregateUpdater>();
            _packageDownloadCountIncrementer = new Infrastructure.Impl.PackageDownloadCountIncrementer(_mockedPackageRepository.Object,
                _mockedPublishedPackageRepository.Object, _mockedPackageDataAggregateUpdater.Object, new Mock<IPackageLogEntryCreator>().Object,
                new Mock<IRepository<PackageLogEntry>>().Object);

            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] {_existingPackage}.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] {_existingPublishedPackage}.AsQueryable());
        }

        [Test]
        public void ShouldGetCollectionOfPackagesFromPackageRepository()
        {
            _packageDownloadCountIncrementer.Increment(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageRepository.VerifyGet(pr => pr.Collection, Times.Once(), "Should have called PackageRepository's Collection.");
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidPackageId()
        {
            const string nonexistentPackageId = "aaa";

            TestDelegate methodThatShouldThrow = () => _packageDownloadCountIncrementer.Increment(nonexistentPackageId, _existingPackage.Version);

            Assert.Throws<ObjectDoesNotExistException<Package>>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldCallUpdatePassingInRetrievedPackage()
        {
            _packageDownloadCountIncrementer.Increment(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageRepository.Verify(pr => pr.Update(_existingPackage), Times.Once(), "Should have updated given Package.");
        }

        [Test]
        public void ShouldIncrementDownloadCountOfExistingPackageByOne()
        {
            int initialDownloadCount = _existingPackage.DownloadCount;
            int expectedDownloadCount = initialDownloadCount + 1;

            _packageDownloadCountIncrementer.Increment(_existingPackage.Id, _existingPackage.Version);

            int newDownloadCount = _existingPackage.DownloadCount;
            Assert.AreEqual(expectedDownloadCount, newDownloadCount, "Package's DownloadCount should have been incremented.");
        }

        [Test]
        public void ShouldIncrementDownloadCountOfExistingPublishedPackageByOne()
        {
            int initialDownloadCount = _existingPublishedPackage.VersionDownloadCount;
            int expectedDownloadCount = initialDownloadCount + 1;

            _packageDownloadCountIncrementer.Increment(_existingPublishedPackage.Id, _existingPublishedPackage.Version);

            int newDownloadCount = _existingPublishedPackage.VersionDownloadCount;
            Assert.AreEqual(expectedDownloadCount, newDownloadCount, "PublishedPackage's VersionDownloadCount should have been incremented.");
        }

        [Test]
        public void ShouldGetCollectionOfPublishedPackagesFromPublishedPackageRepository()
        {
            _packageDownloadCountIncrementer.Increment(_existingPublishedPackage.Id, _existingPublishedPackage.Version);

            _mockedPublishedPackageRepository.VerifyGet(pr => pr.Collection, Times.Once(), "Should have called PublishedPackageRepository's Collection.");
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidPublishedPackageId()
        {
            string nonexistentPublishedPackageId = _existingPackage.Id;
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new PublishedPackage[0].AsQueryable());

            TestDelegate methodThatShouldThrow = () => _packageDownloadCountIncrementer.Increment(nonexistentPublishedPackageId,
                _existingPublishedPackage.Version);

            Assert.Throws<ObjectDoesNotExistException<PublishedPackage>>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldCallUpdatePassingInRetrievedPublishedPackage()
        {
            _packageDownloadCountIncrementer.Increment(_existingPublishedPackage.Id, _existingPublishedPackage.Version);

            _mockedPublishedPackageRepository.Verify(pr => pr.Update(_existingPublishedPackage), Times.Once(), "Should have updated given PublishedPackage.");
        }

        [Test]
        public void ShouldIncrementDownloadCountForPackageAggregate()
        {
            _packageDownloadCountIncrementer.Increment(_existingPublishedPackage.Id, _existingPublishedPackage.Version);

            _mockedPackageDataAggregateUpdater.Verify(pdau => pdau.IncrementDownloadForPackage(_existingPublishedPackage.Id), Times.Once(),
                "Aggregate Download Count was not incremented.");
        }

        //[Test]
        //public void ShouldCreateAndSavePackageLogEntry()
        //{
        //    PublishedPackage package = new PublishedPackage { Id = "Id", Version = "Version", ExternalPackageUrl = "http://foo.com" };
        //    _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[] { package }.AsQueryable());

        //    PackageLogEntry returnedPackageLogEntry = new PackageLogEntry();

        //    _packageLogEntryCreator.Setup(plec => plec.Create(package.Id, package.Version, PackageLogAction.Download))
        //        .Returns(returnedPackageLogEntry).Verifiable("No package log entry was created.");
        //    _packageLogEntryRepository.Setup(pler => pler.Create(returnedPackageLogEntry)).Verifiable("No package log entry was persisted.");

        //    _packageController.Download(package.Id, package.Version);

        //    _packageLogEntryCreator.Verify();
        //}
    }
}