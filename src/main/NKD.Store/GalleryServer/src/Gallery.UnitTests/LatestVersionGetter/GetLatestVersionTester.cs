using System;
using Gallery.Core.Contants;
using Gallery.Core.Domain;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.LatestVersionGetter
{
    [TestFixture]
    public class GetLatestVersionTester
    {
        private Mock<IRepository<Package>> _mockRepo;
        private LatestVersionGetter<Package> _getter;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IRepository<Package>>();
            _getter = new LatestVersionGetter<Package>(_mockRepo.Object);
        }

        private static Package GetPackage(string id, string version = null, bool published = true)
        {
            DateTime publishedDate = Constants.UnpublishedDate;
            if (published)
            {
                publishedDate = DateTime.UtcNow;
            }
            return new Package {Id = id, Version = version, Published = publishedDate};
        }

        [Test]
        public void ShouldCallVersionableRepositoryToGetCollection()
        {
            _getter.GetLatestVersion("packageId");
            _mockRepo.VerifyGet(r => r.Collection, Times.Once());
        }

        [Test]
        public void ShouldReturnNullWhenNoPackagesExistWithGivenId()
        {
            const string packageId = "packageId";
            _mockRepo.SetupGet(r => r.Collection).Returns(new[] {GetPackage(packageId + "2")}.AsQueryable());

            Package latestVersion = _getter.GetLatestVersion(packageId);

            Assert.IsNull(latestVersion);
        }

        [Test]
        public void WhenOnlyOnePackageWithGivenIdExistsItShouldBeReturned()
        {
            var expectedPackage = GetPackage("packageId", "1.1");
            _mockRepo.SetupGet(r => r.Collection).Returns(new[] { expectedPackage }.AsQueryable());

            Package latestVersion = _getter.GetLatestVersion(expectedPackage.Id);

            Assert.AreEqual(expectedPackage, latestVersion);
        }

        [Test]
        public void WhenMoreThanOnePackageWithGivenIdExistsItShouldReturnPackageWithHighestVersion()
        {
            const string packageId = "packageId";
             _mockRepo.SetupGet(r => r.Collection).Returns(new[]
            {
                GetPackage(packageId, "1.0"),
                GetPackage(packageId, "2.01"),
                GetPackage(packageId, "1.5"),
                GetPackage("otherId", "2.1"),
                GetPackage(packageId, "2.0"),
            }.AsQueryable());

            Package latestVersion = _getter.GetLatestVersion(packageId);

            Assert.AreEqual("2.01", latestVersion.Version);
        }

        [Test]
        public void ShouldNotIncludeUnpublishedPackages()
        {
            const string packageId = "packageId";
            const string expectedVersion = "1.5";
            _mockRepo.SetupGet(r => r.Collection).Returns(new[]
            {
                GetPackage(packageId, "1.0"),
                GetPackage(packageId, "2.0", false),
                GetPackage(packageId, expectedVersion),
            }.AsQueryable());

            Package latestVersion = _getter.GetLatestVersion(packageId);

            Assert.AreEqual(expectedVersion, latestVersion.Version);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhitespacePackageId(string packageId)
        {
            TestDelegate methodThatShouldThrow = () => _getter.GetLatestVersion(packageId);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow);
        }
    }
}