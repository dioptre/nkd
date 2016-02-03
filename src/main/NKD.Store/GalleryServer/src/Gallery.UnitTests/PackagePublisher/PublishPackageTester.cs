using System;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackagePublisher
{
    [TestFixture]
    public class PublishPackageTester
    {
        private IPackagePublisher _packagePublisher;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;
        private Mock<IRepository<PublishedScreenshot>> _mockedPublishedScreenshotRepository;
        private Mock<IPackageAuthenticator> _mockedPackageAuthenticator;
        private Mock<IPackageLogEntryCreator> _mockedPackageLogEntryCreator;
        private Mock<ILatestVersionUpdater<PublishedPackage>> _mockedLatestVersionUpdater;
        private Mock<IMapper> _mockedMapper;
        private Mock<IDateTime> _mockedDateTime;

        private const string EXISTING_PACKAGE_ID = "existingId";
        private const string EXISTING_PACKAGE_VERSION = "4.2";
        private const PackageLogAction LOG_ACTION = PackageLogAction.Update;
        private readonly Package _existingPackage = new Package { Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION };

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            _mockedPublishedScreenshotRepository = new Mock<IRepository<PublishedScreenshot>>();
            _mockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            _mockedPackageLogEntryCreator = new Mock<IPackageLogEntryCreator>();
            _mockedLatestVersionUpdater = new Mock<ILatestVersionUpdater<PublishedPackage>>();
            _mockedMapper = new Mock<IMapper>();
            _mockedDateTime = new Mock<IDateTime>();

            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { _existingPackage }.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { new PublishedPackage() }.AsQueryable());
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(It.IsAny<Package>())).Returns(new PublishedPackage());

            _packagePublisher = new Infrastructure.Impl.PackagePublisher(_mockedPackageRepository.Object, _mockedPublishedPackageRepository.Object,
                _mockedPublishedScreenshotRepository.Object, _mockedPackageAuthenticator.Object,
                _mockedPackageLogEntryCreator.Object, _mockedMapper.Object, _mockedDateTime.Object, _mockedLatestVersionUpdater.Object);
        }

        [Test]
        public void ShouldEnsureThatGivenKeyCanAccessPackage()
        {
            const string key = "a key";

            _packagePublisher.PublishPackage(key, EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackage(key, EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION), Times.Once(),
                "Should have made sure that the given key could access the given Package.");
        }

        [Test]
        public void ShouldEnsureThatAnInvalidKeyThrowsExceptionWhenAccesingAPackage()
        {
            const string key = "a key";
            const string packageId = "an id";
            const string packageVersion = "a version";
            var expectedException = new Exception();
            _mockedPackageAuthenticator.Setup(pa => pa.EnsureKeyCanAccessPackage(key, packageId, packageVersion)).Throws(expectedException);

            TestDelegate methodThatShouldThrow = () => _packagePublisher.PublishPackage(key, packageId, packageVersion, LOG_ACTION);

            ExceptionAssert.Throws(methodThatShouldThrow, expectedException, "Should have thrown an exception.");
        }

        [Test]
        public void ShouldFetchExistingPackageFromPackageRepository()
        {
            _packagePublisher.PublishPackage("some", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPackageRepository.VerifyGet(pr => pr.Collection, Times.Once(), "Collection should have been retrieved.");
        }

        [Test]
        public void ShouldThrowWhenGivenPackageDoesNotExist()
        {
            const string nonExistentVersion = "1.3";

            TestDelegate methodThatShouldThrow = () => _packagePublisher.PublishPackage("key", EXISTING_PACKAGE_ID, nonExistentVersion, LOG_ACTION);

            Assert.Throws<PackageDoesNotExistException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldMapPackageToPublishedPackage()
        {
            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedMapper.Verify(m => m.Map<Package, PublishedPackage>(_existingPackage), Times.Once(), "Should have mapped package to published package.");
        }

        [Test]
        public void ShouldNotDeleteExistingPublishedPackageWhenPackageHasNotPreviouslyBeenPublished()
        {
            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPublishedPackageRepository.Verify(ppr => ppr.DeleteMany(It.IsAny<Func<PublishedPackage, bool>>()), Times.Never());
        }

        [Test]
        public void ShouldDeleteExistingPublishedPackageWhenPackageHasPreviouslyBeenPublished()
        {
            _mockedPublishedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION } }.AsQueryable());
            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPublishedPackageRepository.Verify(ppr => ppr.DeleteMany(It.IsAny<Func<PublishedPackage, bool>>()), Times.Once());
        }

        [Test]
        public void ShouldNotDeleteExistingPublishedScreenshotsWhenPackageHasNotPreviouslyBeenPublished()
        {
            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPublishedScreenshotRepository.Verify(psr => psr.DeleteMany(It.IsAny<Func<PublishedScreenshot, bool>>()), Times.Never());
        }

        [Test]
        public void ShouldDeleteExistingPublishedScreenshotsWhenPackageHasPreviouslyBeenPublished()
        {
            _mockedPublishedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { new PublishedPackage {Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION } }.AsQueryable());
            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPublishedScreenshotRepository.Verify(psr => psr.DeleteMany(It.IsAny<Func<PublishedScreenshot, bool>>()), Times.Once());
        }

        [Test]
        public void ShouldSaveMappedPublishedPackage()
        {
            var publishedPackageToCreate = new PublishedPackage();
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPublishedPackageRepository.Verify(ppr => ppr.Create(publishedPackageToCreate), Times.Once(),
                "Mapped PublishedPackage should have been created.");
        }

        [Test]
        public void ShouldCreateAPackageLogEntryUsingSuppliedLogActionWhenPackageHasAlreadyBeenPublished()
        {
            var publishedPackageToCreate = new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION };
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);
            _mockedPublishedPackageRepository.SetupGet(r => r.Collection).Returns(new[] {publishedPackageToCreate}.AsQueryable());

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPackageLogEntryCreator.Verify(c => c.Create(EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION), Times.Once(),
                "PackageLogEntryCreator's Create() should have been called exactly once for an Update action with the expected package ID and version.");
        }

        [Test]
        public void ShouldCreateAPackageLogEntryForCreateWhenPackageHasNotBeenPublished()
        {
            var publishedPackageToCreate = new PublishedPackage();
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedPackageLogEntryCreator.Verify(c => c.Create(EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, PackageLogAction.Create), Times.Once(),
                "PackageLogEntryCreator's Create() should have been called exactly once for a Create action with the expected package ID and version.");
        }

        [Test]
        public void ShouldCallLatestVersionUpdaterWhenPackageToPublishIsLatestVersionAndAnotherPackageWithSameIdAlreadyIsTheLastestVersion()
        {
            var publishedPackageToCreate = new PublishedPackage { Id = EXISTING_PACKAGE_ID, IsLatestVersion = true };
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[]
            {
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.5.6.2", IsLatestVersion = true},
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.6.6.2", IsLatestVersion = false}
            }.AsQueryable());

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedLatestVersionUpdater.Verify(lvu => lvu.SetLatestVersionFlagsOfOtherVersionablesWithSameId(publishedPackageToCreate), Times.Once(),
                "LatestVersionUpdater should have been called.");
        }

        [Test]
        public void ShouldNotCallLatestVersionUpdaterWhenPackageToPublishIsNotLatestVersion()
        {
            var publishedPackageToCreate = new PublishedPackage { IsLatestVersion = false };
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            _mockedLatestVersionUpdater.Verify(lvu => lvu.SetLatestVersionFlagsOfOtherVersionablesWithSameId(It.IsAny<PublishedPackage>()), Times.Never(),
                "LatestVersionUpdater should not have been called.");
        }

        [Test]
        public void ShouldSetIsLatestVersionToTrueWhenNoOtherPublishedPackageWithSameIdIsTheLatestVersion()
        {
            var publishedPackageToCreate = new PublishedPackage { IsLatestVersion = false };
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[]
            {
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.5.6.2", IsLatestVersion = false},
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.6.6.2", IsLatestVersion = false}
            }.AsQueryable());

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            Assert.IsTrue(publishedPackageToCreate.IsLatestVersion, "IsLatestVersion should have been set to true for the package to publish.");
        }

        [Test]
        public void ShouldNotSetIsLatestVersionToTrueWhenAnotherPublishedPackageWithSameIdIsTheLatestVersion()
        {
            var publishedPackageToCreate = new PublishedPackage { Id = EXISTING_PACKAGE_ID, IsLatestVersion = false };
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(_existingPackage)).Returns(publishedPackageToCreate);
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[]
            {
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.5.6.2", IsLatestVersion = true},
                new PublishedPackage { Id = EXISTING_PACKAGE_ID, Version = "1.6.6.2", IsLatestVersion = false}
            }.AsQueryable());

            _packagePublisher.PublishPackage("newKey", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, LOG_ACTION);

            Assert.IsFalse(publishedPackageToCreate.IsLatestVersion, "IsLatestVersion should have remained false for the package to publish.");
        }
    }
}