using System;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.LatestVersionChecker
{
    [TestFixture]
    public class IsLatestVersionTester
    {
        private ILatestVersionChecker _latestVersionChecker;

        private Mock<IRepository<Package>> _mockedPackageRepository;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();

            _latestVersionChecker = new Core.Impl.LatestVersionChecker(_mockedPackageRepository.Object);
        }

        [Test]
        public void ShouldSetIsLatestVersionToTrueWhenSubmittedPackageIsTheFirstForItsId()
        {
            const string packageId = "PackageId";
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new Package[0].AsQueryable());

            bool isLatestVersion = _latestVersionChecker.IsLatestVersion(packageId, "1.2.3.4");

            Assert.IsTrue(isLatestVersion, "IsLatestVersion should have been set to true.");
        }

        [Test]
        public void ShouldSetIsLatestVersionToTrueWhenAllOtherVersionsOfSameIdAreLessThanSubmittedVersion()
        {
            const string packageId = "PackageId";
            string packageVersion = new Version(9, 9, 9, 9).ToString();

            var existingPackages = new[]
            {
                new Package { Id = packageId, Version = new Version(8, 8, 8, 8).ToString() },
                new Package { Id = packageId, Version = new Version(7, 7, 7, 7).ToString() }
            };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(existingPackages.AsQueryable());

            bool isLatestVersion = _latestVersionChecker.IsLatestVersion(packageId, packageVersion);

            Assert.IsTrue(isLatestVersion, "IsLatestVersion should have been set to true.");
        }

        [Test]
        public void ShouldSetIsLatestVersionToFalseWhenGivenPackageVersionIsSmallerThanExistingPackageVersion()
        {
            const string packageId = "PackageId";
            string packageVersion = new Version(9, 9, 9, 9).ToString();

            var existingPackages = new[]
            {
                new Package { Id = packageId, Version = new Version(18, 18, 18, 18).ToString() },
                new Package { Id = packageId, Version = new Version(7, 7, 7, 7).ToString() }
            };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(existingPackages.AsQueryable());

            bool isLatestVersion = _latestVersionChecker.IsLatestVersion(packageId, packageVersion);

            Assert.IsFalse(isLatestVersion, "IsLatestVersion should have been set to false since a greater version already exists.");
        }
    }
}