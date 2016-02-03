using System;
using System.IO;
using Gallery.Core;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageCreator
{
    [TestFixture]
    public class CreatePackageTester
    {
        private IPackageCreator _packageCreator;

        private Mock<IPackageIdValidator> _mockedPackageIdValidator;
        private Mock<IPackageFactory> _mockedPackageFactory;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<ILatestVersionChecker> _mockedLatestVersionChecker;
        private Mock<ILatestVersionUpdater<Package>> _mockedLatestVersionUpdater;
        private Mock<IPackageUriValidator> _mockedPackageUriValidator;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageIdValidator = new Mock<IPackageIdValidator>();
            _mockedPackageFactory = new Mock<IPackageFactory>();
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedLatestVersionChecker = new Mock<ILatestVersionChecker>();
            _mockedLatestVersionUpdater = new Mock<ILatestVersionUpdater<Package>>();
            _mockedPackageUriValidator = new Mock<IPackageUriValidator>();
            Mock<IGuid> mockedGuid = new Mock<IGuid>();
            Mock<IConfigSettings> mockedConfigSettings = new Mock<IConfigSettings>();
            Mock<IHashGetter> mockedHashGetter = new Mock<IHashGetter>();

            _packageCreator = new Core.Impl.PackageCreator(new Mock<IFileSystem>().Object, mockedGuid.Object, _mockedPackageFactory.Object,
                mockedConfigSettings.Object, _mockedPackageRepository.Object, new Mock<IPackageAuthenticator>().Object, mockedHashGetter.Object,
                _mockedPackageIdValidator.Object, _mockedPackageUriValidator.Object, _mockedLatestVersionChecker.Object, _mockedLatestVersionUpdater.Object);

            mockedGuid.Setup(g => g.NewGuid()).Returns(Guid.NewGuid());
            mockedConfigSettings.SetupGet(cs => cs.PhysicalSitePath).Returns("physical");
            mockedConfigSettings.SetupGet(cs => cs.RelativeTemporaryDirectory).Returns("temp");
            mockedConfigSettings.SetupGet(cs => cs.RelativePackageDirectory).Returns("package");
            mockedHashGetter.Setup(hg => hg.GetHashFromFile(It.IsAny<Stream>())).Returns(new ComputedHash { HashingAlgorithmUsed = HashingAlgorithm.SHA1});
            _mockedPackageIdValidator.Setup(piv => piv.IsValidPackageId(It.IsAny<string>())).Returns(true);
        }

        [Test]
        public void ShouldCallPackageIdValidator()
        {
            var package = new Package { Id = "Package"};
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(package);

            _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            _mockedPackageIdValidator.Verify(piv => piv.IsValidPackageId(package.Id), Times.Once(), "PackageIdValidator was not called.");
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidPackageId()
        {
            Package package = new Package { Id = "PackageId"};
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(package);
            _mockedPackageIdValidator.Setup(piv => piv.IsValidPackageId(package.Id)).Returns(false);

            TestDelegate methodThatShouldThrow = () => _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            Assert.Throws<InvalidPackageIdException>(methodThatShouldThrow, "Expected exception was not thrown when given invalid Package ID.");
        }

        [Test]
        public void ShouldCallLatestVersionCheckerToSetIsLatestVersionOnPackage()
        {
            const string packageId = "PackageId";
            const string packageVersion = "1.2.3.4";
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(new Package { Id = packageId, Version = packageVersion });
            _mockedPackageIdValidator.Setup(piv => piv.IsValidPackageId(packageId)).Returns(true);
            const bool expected = true;
            _mockedLatestVersionChecker.Setup(lvc => lvc.IsLatestVersion(packageId, packageVersion)).Returns(expected)
                .Verifiable("Should have called LatestVersionChecker.");

            Package package = _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            Assert.AreEqual(expected, package.IsLatestVersion, "IsLatestVersion should be set to what LatestVersionChecker returns.");
        }

        [Test]
        public void ShouldCallLatestVersionUpdater()
        {
            const string packageId = "PackageId";
            Package createdPackage = new Package { Id = packageId };
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(createdPackage);
            _mockedPackageIdValidator.Setup(piv => piv.IsValidPackageId(packageId)).Returns(true);

            _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            _mockedLatestVersionUpdater.Verify(lvu => lvu.SetLatestVersionFlagsOfOtherVersionablesWithSameId(createdPackage), Times.Once(),
                "LatestVersionUpdater was not called.");
        }

        [Test]
        public void ShouldValidatePackageUrisWithPackageUriValidator()
        {
            var package = new Package();
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(package);

            _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            _mockedPackageUriValidator.Verify(puv => puv.ValidatePackageUris(package), Times.Once());
        }

        [Test]
        public void ShouldThrowWhenPackageUriValidatorThrows()
        {
            _mockedPackageFactory.Setup(pf => pf.CreateNewFromFile(It.IsAny<string>())).Returns(new Package());
            _mockedPackageUriValidator.Setup(puv => puv.ValidatePackageUris(It.IsAny<Package>())).Throws(new UriFormatException());

            TestDelegate methodThatShouldThrow = () => _packageCreator.CreatePackage("key", Stream.Null, "fileExtension", false);

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }

    }
}