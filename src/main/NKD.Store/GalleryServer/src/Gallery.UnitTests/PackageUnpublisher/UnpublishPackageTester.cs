using System;
using System.Linq;
using Gallery.Core.Contants;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageUnpublisher
{
    [TestFixture]
    public class UnpublishPackageTester
    {
        private IPackageUnpublisher _packageUnpublisher;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;
        private Mock<IPackageAuthenticator> _mockedPackageAuthenticator;
        private Mock<IMapper> _mockedMapper;
        private Mock<IRecommendedVersionManager<Package>> _mockedPackageRecommendedVersionManager;
        private Mock<IRecommendedVersionManager<PublishedPackage>> _mockedPublishedPackageRecommendedVersionManager;
        private Mock<IPackageLogEntryCreator> _mockedPackageLogEntryCreator;

        private const string EXISTING_PACKAGE_ID = "existingId";
        private const string EXISTING_PACKAGE_VERSION = "4.2";
        private Package _existingPackage;

        private PublishedPackage _existingPublishedPackage;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            _mockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            _mockedPackageRecommendedVersionManager = new Mock<IRecommendedVersionManager<Package>>();
            _mockedPublishedPackageRecommendedVersionManager = new Mock<IRecommendedVersionManager<PublishedPackage>>();
            _mockedMapper = new Mock<IMapper>();
            _mockedPackageLogEntryCreator = new Mock<IPackageLogEntryCreator>();

            _existingPackage = new Package {Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION, Published = DateTime.Now };
            _existingPublishedPackage = new PublishedPackage {Id = EXISTING_PACKAGE_ID, Version = EXISTING_PACKAGE_VERSION, Published = DateTime.Now};
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { _existingPackage }.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { _existingPublishedPackage }.AsQueryable());
            _mockedMapper.Setup(m => m.Map<Package, PublishedPackage>(It.IsAny<Package>())).Returns(new PublishedPackage());

            _packageUnpublisher = new Infrastructure.Impl.PackageUnpublisher(_mockedPackageRepository.Object, _mockedPublishedPackageRepository.Object,
                _mockedPackageAuthenticator.Object, _mockedPackageRecommendedVersionManager.Object, _mockedPublishedPackageRecommendedVersionManager.Object,
                _mockedPackageLogEntryCreator.Object);
        }

        [Test]
        public void ShouldEnsureThatGivenKeyCanAccessPackage()
        {
            const string key = "a key";

            _packageUnpublisher.UnpublishPackage(key, EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackage(key, EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION), Times.Once(),
                "Should have made sure that the given key could access the given Package.");
        }

        [Test]
        public void ShouldEnsureThatAnInvalidKeyThrowsExceptionWhenAccesingAPackage()
        {
            const string key = "a key";
            const string packageId = "an id";
            const string packageVersion = "a version";
            Exception expectedException = new Exception();
            _mockedPackageAuthenticator.Setup(pa => pa.EnsureKeyCanAccessPackage(key, packageId, packageVersion)).Throws(expectedException);

            TestDelegate methodThatShouldThrow = () => _packageUnpublisher.UnpublishPackage(key, packageId, packageVersion);

            ExceptionAssert.Throws(methodThatShouldThrow, expectedException, "Should have thrown an exception.");
        }

        [Test]
        public void ShouldFetchExistingPackageFromPackageRepository()
        {
            _packageUnpublisher.UnpublishPackage("some", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageRepository.VerifyGet(pr => pr.Collection, Times.Once(), "Collection should have been retrieved.");
        }

        [Test]
        public void ShouldThrowWhenGivenPackageDoesNotExist()
        {
            const string nonExistentVersion = "1.3";

            TestDelegate methodThatShouldThrow = () => _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, nonExistentVersion);

            Assert.Throws<PackageDoesNotExistException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldThrowWhenExistingPackageHasNotBeenPublished()
        {
            _existingPackage.Published = Constants.UnpublishedDate;

            TestDelegate methodThatShouldThrow = () => _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.Throws<InvalidOperationException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldThrowWhenPublishedPackageDoesNotExist()
        {
            _existingPublishedPackage.Id = Guid.NewGuid().ToString();

            TestDelegate methodThatShouldThrow = () => _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.Throws<InvalidOperationException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldThrowWhenPublishedPackagePublishedDateIsUnpublishedConstant()
        {
            _existingPublishedPackage.Published = Constants.UnpublishedDate;

            TestDelegate methodThatShouldThrow = () => _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.Throws<InvalidOperationException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldSetPublishedToUnpublishedConstantOnPackage()
        {
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.AreEqual(Constants.UnpublishedDate, _existingPackage.Published, "Published field on Package should have been nullified.");
        }

        [Test]
        public void ShouldSetIsLatestVersionToFalseOnPackage()
        {
            _existingPackage.IsLatestVersion = true;

            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.IsFalse(_existingPackage.IsLatestVersion, "IsLatestVersion on Package should have been set to false.");
        }

        [Test]
        public void ShouldCallUpdateOnPackageRepository()
        {
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageRepository.Verify(p => p.Update(_existingPackage), Times.Once(),
                "Update was not called on Package with null Published date.");
        }

        [Test]
        public void ShouldSetPublishedToUnpublishedConstantOnPublishedPackage()
        {
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.AreEqual(Constants.UnpublishedDate, _existingPublishedPackage.Published, "Published field on Published Package should have been nullified.");
        }

        [Test]
        public void ShouldSetIsLatestVersionToFalseOnPublishedPackage()
        {
            _existingPackage.IsLatestVersion = true;
            _existingPublishedPackage.IsLatestVersion = true;

            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            Assert.IsFalse(_existingPublishedPackage.IsLatestVersion, "IsLatestVersion on PublishedPackage should have been set to false.");
        }

        [Test]
        public void ShouldCallUpdateOnPublishedPackageRepository()
        {
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPublishedPackageRepository.Verify(p => p.Update(_existingPublishedPackage), Times.Once(),
                "Update was not called on Published Package with null Published date.");
        }

        [Test]
        public void ShouldNotCallRecommendedVersionManagerWhenNotUnpublishingTheRecommendedVersion()
        {
            _existingPackage.IsLatestVersion = false;
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageRecommendedVersionManager.Verify(rvm => rvm.SetLatestVersionAsRecommended(EXISTING_PACKAGE_ID, false), Times.Never());
            _mockedPublishedPackageRecommendedVersionManager.Verify(rvm => rvm.SetLatestVersionAsRecommended(EXISTING_PACKAGE_ID, true), Times.Never());
        }

        [Test]
        public void ShouldCallRecommendedVersionManagerWhenUnpublishingTheRecommendedVersion()
        {
            _existingPackage.IsLatestVersion = true;
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageRecommendedVersionManager.Verify(rvm => rvm.SetLatestVersionAsRecommended(EXISTING_PACKAGE_ID, false), Times.Once());
            _mockedPublishedPackageRecommendedVersionManager.Verify(rvm => rvm.SetLatestVersionAsRecommended(EXISTING_PACKAGE_ID, true), Times.Once());
        }

        [Test]
        public void ShouldCreateUnpublishLogAction()
        {
            _packageUnpublisher.UnpublishPackage("key", EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(EXISTING_PACKAGE_ID, EXISTING_PACKAGE_VERSION, PackageLogAction.Unpublish), Times.Once());
        }
    }
}