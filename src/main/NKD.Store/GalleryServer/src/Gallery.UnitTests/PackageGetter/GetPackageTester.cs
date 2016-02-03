using System;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageGetter
{
    [TestFixture]
    public class GetPackageTester
    {
        private IPackageGetter _packageGetter;
        private Mock<IPackageAuthenticator> _mockedPackageAuthenticator;
        private Mock<IRepository<Package>> _mockedPackageRepository;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _packageGetter = new Infrastructure.Impl.PackageGetter(_mockedPackageAuthenticator.Object, _mockedPackageRepository.Object);
        }

        [Test]
        public void ShouldEnsureGivenKeyHasAccessToPackage()
        {
            string key = Guid.NewGuid().ToString();
            const string id = "id";
            const string version = "version";
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(new[] { new Package { Id = id, Version = version } }.AsQueryable());

            _packageGetter.GetPackage(key, id, version);

            _mockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackage(key, id, version), Times.Once(),
                "PackageAuthenticator's EnsureKeyCanAccessPackage should have been invoked with the specified arguments.");
        }

        [Test]
        public void ShouldThrowWhenAuthenticatorThrows()
        {
            _mockedPackageAuthenticator.Setup(pa => pa.EnsureKeyCanAccessPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PackageAuthorizationException());

            TestDelegate methodThatShouldThrow = () => _packageGetter.GetPackage("key", "id", "version");

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Exception should have been thrown since PackageAuthenticator threw.");
        }

        [Test]
        public void ShouldGetCollectionFromPackageRepository()
        {
            const string id = "foo";
            const string version = "bar";
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(new[] { new Package { Id = id, Version = version } }.AsQueryable());

            _packageGetter.GetPackage("", id, version);

            _mockedPackageRepository.Verify(pr => pr.Collection, Times.Once(), "Collection on PackageRepository should have been accessed once.");
        }

        [Test]
        public void ShouldReturnSingleMatchedPackage()
        {
            const string packageId = "id";
            const string packageVersion = "version";
            Package expectedPackage = new Package { Id = packageId, Version = packageVersion };
            IQueryable<Package> collectionPackages = new[] { expectedPackage, new Package(), new Package() }.AsQueryable();
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(collectionPackages);

            Package package = _packageGetter.GetPackage("", packageId, packageVersion);

            Assert.AreEqual(expectedPackage, package, "Incorrect Package returned.");
        }

        [Test]
        public void ShouldThrowWhenPackageWithGivenIdAndVersionDoesNotExist()
        {
            IQueryable<Package> collectionPackages = new[] { new Package(), new Package(), new Package() }.AsQueryable();
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(collectionPackages);

            TestDelegate methodThatShouldThrow = () => _packageGetter.GetPackage("", "id", "version");

            Assert.Throws<PackageDoesNotExistException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldThrowWhenMultiplePackagesExistWithSameIdAndVersion()
        {
            const string id = "id";
            const string verison = "version";
            Package duplicatePackage = new Package { Id = id, Version = verison };
            IQueryable<Package> collectionPackages = new[] { duplicatePackage, duplicatePackage }.AsQueryable();
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(collectionPackages);

            TestDelegate methodThatShouldThrow = () => _packageGetter.GetPackage("", "id", "version");

            ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow);
        }
    }
}