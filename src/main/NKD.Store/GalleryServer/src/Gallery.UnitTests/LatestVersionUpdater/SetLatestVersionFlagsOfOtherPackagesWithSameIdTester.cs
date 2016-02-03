using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.LatestVersionUpdater
{
    [TestFixture]
    public class SetLatestVersionFlagsOfOtherPackagesWithSameIdTester
    {
        private ILatestVersionUpdater<Package> _latestVersionUpdater;

        private Mock<IRepository<Package>> _mockedPackageRepository;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();

            _latestVersionUpdater = new Core.Impl.LatestVersionUpdater<Package>(_mockedPackageRepository.Object);
        }

        [Test]
        public void ShouldNotInvokeRepositoryWhenGivenPackageIsNotLatestVersion()
        {
            Package package = new Package {IsLatestVersion = false};

            _latestVersionUpdater.SetLatestVersionFlagsOfOtherVersionablesWithSameId(package);

            _mockedPackageRepository.VerifyGet(pr => pr.Collection, Times.Never(), "PackageRepository should not have been invoked.");
            _mockedPackageRepository.Verify(pr => pr.Update(It.IsAny<IEnumerable<Package>>()), Times.Never(), "No Update calls should have been made.");
        }

        [Test]
        public void ShouldSetOtherPackagesToNotBeLatestVersion()
        {
            Package package = GetPackage("Some ID", true);
            Package otherPackageThatWasLatest = GetPackage(package.Id, true);
            Package otherPackageThatWasNotLatest = GetPackage(package.Id, false);
            IQueryable<Package> existingPackages = new[] { package, otherPackageThatWasLatest, otherPackageThatWasNotLatest}.AsQueryable();
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(existingPackages);

            _latestVersionUpdater.SetLatestVersionFlagsOfOtherVersionablesWithSameId(package);

            _mockedPackageRepository.Verify(pr => pr.Update(It.IsAny<IEnumerable<Package>>()), Times.Once(), "Update should have been invoked.");
            Assert.IsTrue(package.IsLatestVersion, "The passed-in Package should not have IsLatestVersion altered.");
            Assert.IsFalse(otherPackageThatWasLatest.IsLatestVersion, "Other Package that was latest should have had been changed to no longer be latest.");
            Assert.IsFalse(otherPackageThatWasLatest.IsLatestVersion, "Other Package that was not marked as latest should not have been marked as latest.");
        }

        private static Package GetPackage(string packageId, bool isLatestVersion)
        {
            return new Package {Id = packageId, Version = Guid.NewGuid().ToString(), IsLatestVersion = isLatestVersion};
        }
    }
}