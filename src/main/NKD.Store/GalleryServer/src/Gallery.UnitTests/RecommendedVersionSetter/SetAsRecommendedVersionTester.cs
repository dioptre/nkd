using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.RecommendedVersionSetter
{
    [TestFixture]
    public class SetAsRecommendedVersionTester
    {
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IPackageLogEntryCreator> _mockedPackageLogEntryCreator = new Mock<IPackageLogEntryCreator>();
        private IRecommendedVersionSetter<Package> _setter;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedPackageLogEntryCreator = new Mock<IPackageLogEntryCreator>();
            _setter = new RecommendedVersionSetter<Package>(_mockedPackageRepository.Object,
                _mockedPackageLogEntryCreator.Object);
        }

        [Test]
        public void ShouldNotThrowWhenGivenNullPackage()
        {
            Package nullPackage = null;

            TestDelegate methodThatShouldNotThrow = () => _setter.SetAsRecommendedVersion(nullPackage, true);

            Assert.DoesNotThrow(methodThatShouldNotThrow, "A null Package should not throw an exception.");
        }

        [Test]
        public void ShouldCallVersionRepositoryToGetCollection()
        {
            Package package = new Package {Id = "Foo", Version = "1.0"};

            _setter.SetAsRecommendedVersion(package, true);

            _mockedPackageRepository.VerifyGet(pr => pr.Collection, Times.Once());
        }

        [Test]
        public void ShouldSetGivenVersionAsRecommended()
        {
            Package package = new Package { Id = "Foo", Version = "1.0", IsLatestVersion = false};
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(new[]{ package }.AsQueryable());

            _setter.SetAsRecommendedVersion(package, true);

            _mockedPackageRepository.Verify(pr => pr.Update(package), Times.Once());
            Assert.True(package.IsLatestVersion);
        }

        [Test]
        public void ShouldCreateLogEntryForGivenVersionWhenToldTo()
        {
            Package packageToRecommend = new Package { Id = "Foo", Version = "1.1", IsLatestVersion = false };

            _setter.SetAsRecommendedVersion(packageToRecommend, true);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageToRecommend.Id, packageToRecommend.Version, PackageLogAction.Update), Times.Once());
        }

        [Test]
        public void ShouldNotLogEntryForGivenVersionWhenToldNotTo()
        {
            Package packageToRecommend = new Package { Id = "Foo", Version = "1.1", IsLatestVersion = false };

            _setter.SetAsRecommendedVersion(packageToRecommend, false);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageToRecommend.Id, packageToRecommend.Version, PackageLogAction.Update), Times.Never());
        }

        [Test]
        public void ShouldCreateLogEntriesForPackagesSetAsNotRecommendedWhenToldTo()
        {
            const string packageId = "Foo";
            Package packageToRecommend = new Package {Id = packageId, Version = "1.1", IsLatestVersion = false};
            Package packageToUnrecommend1 = new Package { Id = packageId, Version = "1.0", IsLatestVersion = true };
            Package packageToUnrecommend2 = new Package { Id = packageId, Version = "1.05", IsLatestVersion = true };
            _mockedPackageRepository
                .SetupGet(pr => pr.Collection)
                .Returns(new[] {packageToRecommend, packageToUnrecommend1, packageToUnrecommend2}.AsQueryable());

            _setter.SetAsRecommendedVersion(packageToRecommend, true);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageId, packageToUnrecommend1.Version, PackageLogAction.Update), Times.Once());
            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageId, packageToUnrecommend2.Version, PackageLogAction.Update), Times.Once());
        }

        [Test]
        public void ShouldNotCreateLogEntriesForPackagesSetAsNotRecommendedWhenToldNotTo()
        {
            const string packageId = "Foo";
            Package packageToRecommend = new Package { Id = packageId, Version = "1.1", IsLatestVersion = false };
            Package packageToUnrecommend1 = new Package { Id = packageId, Version = "1.0", IsLatestVersion = true };
            Package packageToUnrecommend2 = new Package { Id = packageId, Version = "1.05", IsLatestVersion = true };
            _mockedPackageRepository
                .SetupGet(pr => pr.Collection)
                .Returns(new[] { packageToRecommend, packageToUnrecommend1, packageToUnrecommend2 }.AsQueryable());

            _setter.SetAsRecommendedVersion(packageToRecommend, false);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageId, packageToUnrecommend1.Version, PackageLogAction.Update), Times.Never());
            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(packageId, packageToUnrecommend2.Version, PackageLogAction.Update), Times.Never());
        }

        [Test]
        public void WhenAnotherPackageWithSameIdExistsThatPackageShouldBeUnsetAsRecommended()
        {
            const string packageId = "PackageId";
            Package packageToSetAsRecommended = new Package { Id = packageId, Version = "1.0", IsLatestVersion = false};
            Package packageToUnset = new Package { Id = packageId, Version = "1.1", IsLatestVersion = true};
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { packageToSetAsRecommended, packageToUnset }.AsQueryable());

            _setter.SetAsRecommendedVersion(packageToSetAsRecommended, true);

            _mockedPackageRepository.Verify(pr => pr.Update(packageToUnset), Times.Once());
            Assert.IsTrue(packageToSetAsRecommended.IsLatestVersion);
            Assert.IsFalse(packageToUnset.IsLatestVersion);
        }

        [Test]
        public void WhenOtherPackagesWithSameIdExistThosePackagesShouldBeSetAsNotRecommended()
        {
            const string packageId = "PackageId";
            Package packageToSetAsRecommended = new Package { Id = packageId, Version = "1.0", IsLatestVersion = false };
            Package packageToUnset1 = new Package { Id = packageId, Version = "1.5", IsLatestVersion = true };
            Package packageToUnset2 = new Package { Id = packageId, Version = "1.4", IsLatestVersion = true };
            Package packageToNotTouch = new Package { Id = "otherId", Version = "3.1", IsLatestVersion = true };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[]
            {
                packageToSetAsRecommended, packageToUnset1, packageToUnset2, packageToNotTouch
            }.AsQueryable());

            _setter.SetAsRecommendedVersion(packageToSetAsRecommended, true);

            _mockedPackageRepository.Verify(pr => pr.Update(packageToUnset1), Times.Once());
            _mockedPackageRepository.Verify(pr => pr.Update(packageToUnset2), Times.Once());
            Assert.IsFalse(packageToUnset1.IsLatestVersion);
            Assert.IsFalse(packageToUnset2.IsLatestVersion);
            Assert.IsTrue(packageToNotTouch.IsLatestVersion);
        }
    }
}