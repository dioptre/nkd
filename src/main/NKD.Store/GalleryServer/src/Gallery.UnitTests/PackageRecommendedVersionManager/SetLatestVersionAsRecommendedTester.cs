using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageRecommendedVersionManager
{
    [TestFixture]
    public class SetLatestVersionAsRecommendedTester
    {
        private IRecommendedVersionManager<Package> _manager;
        private Mock<ILatestVersionGetter<Package>> _mockedPackageLatestVersionGetter;
        private Mock<IRecommendedVersionSetter<Package>> _mockedPackageRecommendedVersionSetter;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageLatestVersionGetter = new Mock<ILatestVersionGetter<Package>>();
            _mockedPackageRecommendedVersionSetter = new Mock<IRecommendedVersionSetter<Package>>();

            _manager = new Core.Impl.RecommendedVersionManager<Package>(_mockedPackageLatestVersionGetter.Object,
                _mockedPackageRecommendedVersionSetter.Object);
        }

        [Test]
        public void CallsPackageLatestVersionGetter()
        {
            const string packageId = "FooPackage";

            _manager.SetLatestVersionAsRecommended(packageId, true);

            _mockedPackageLatestVersionGetter.Verify(plvg => plvg.GetLatestVersion(packageId), Times.Once());
        }

        [Test]
        public void CallsPackageRecommendedVersionSetter()
        {
            Package latestVersion = new Package();
            const bool shouldLogPackageAction = true;
            _mockedPackageLatestVersionGetter.Setup(plvg => plvg.GetLatestVersion(It.IsAny<string>())).Returns(latestVersion);


            _manager.SetLatestVersionAsRecommended("foo", shouldLogPackageAction);

            _mockedPackageRecommendedVersionSetter.Verify(prvs => prvs.SetAsRecommendedVersion(latestVersion, shouldLogPackageAction), Times.Once());
        }
    }
}