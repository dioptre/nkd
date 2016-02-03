using System;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageUpdater
{
    [TestFixture]
    public class UpdateExistingPackageTester
    {
        private IPackageUpdater _packageUpdater;

        private Mock<ILatestVersionUpdater<Package>> _mockedLatestVersionUpdater;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IPackageUriValidator> _mockedPackageUriValidator;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedLatestVersionUpdater = new Mock<ILatestVersionUpdater<Package>>();
            _mockedPackageUriValidator = new Mock<IPackageUriValidator>();

            _packageUpdater = new Core.Impl.PackageUpdater(_mockedPackageRepository.Object, _mockedPackageUriValidator.Object,
                _mockedLatestVersionUpdater.Object);
        }

        [Test]
        public void ShouldCallUpdateOnPackageRepository()
        {
            var packageToUpdate = new Package();

            _packageUpdater.UpdateExistingPackage(packageToUpdate);

            _mockedPackageRepository.Verify(pr => pr.Update(packageToUpdate), Times.Once(),
                "PackageRepository's Update() should have been called exactly once with the expected Package.");
        }

        [Test]
        public void ShouldCallLatestVersionUpdater()
        {
            var packageToUpdate = new Package();

            _packageUpdater.UpdateExistingPackage(packageToUpdate);

            _mockedLatestVersionUpdater.Verify(pr => pr.SetLatestVersionFlagsOfOtherVersionablesWithSameId(packageToUpdate), Times.Once(),
                "Should have updated the LatestVersion information for Packages sharing the same ID.");
        }

        [Test]
        public void ShouldValidatePackageUrisWithPackageUriValidator()
        {
            var packageToUpdate = new Package();

            _packageUpdater.UpdateExistingPackage(packageToUpdate);

            _mockedPackageUriValidator.Verify(puv => puv.ValidatePackageUris(packageToUpdate), Times.Once());
        }

        [Test]
        public void ShouldThrowWhenPackageUriValidatorThrows()
        {
            _mockedPackageUriValidator.Setup(puv => puv.ValidatePackageUris(It.IsAny<Package>())).Throws(new UriFormatException());

            TestDelegate methodThatShouldThrow = () => _packageUpdater.UpdateExistingPackage(new Package());

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }
    }
}