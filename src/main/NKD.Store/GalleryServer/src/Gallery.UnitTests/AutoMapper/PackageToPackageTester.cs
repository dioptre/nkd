using Gallery.Core.Domain;
using NUnit.Framework;

namespace Gallery.UnitTests.AutoMapper
{
    public class PackageToPackageTester : AutoMapperTester
    {
        [Test]
        public void ShouldNotMapScreenshotsProperty()
        {
            Package package = new Package {Screenshots = new[] {new Screenshot(), new Screenshot()}};

            Package mappedPackage = Mapper.Map<Package, Package>(package);

            Assert.IsNull(mappedPackage.Screenshots, "Screenshots property should be null (i.e. should not have been mapped).");
        }

        [Test]
        public void ShouldNotMapDownloadCount()
        {
            Package package = new Package {DownloadCount = 45};

            Package mappedPackage = Mapper.Map<Package, Package>(package);

            Assert.AreNotEqual(package.DownloadCount, mappedPackage.DownloadCount, "DownloadCount should not have been mapped.");
        }

        [Test]
        public void ShouldNotMapRatingsCount()
        {
            Package package = new Package { RatingsCount = 99 };

            Package mappedPackage = Mapper.Map<Package, Package>(package);

            Assert.AreNotEqual(package.RatingsCount, mappedPackage.RatingsCount, "RatingsCount should not have been mapped.");
        }

        [Test]
        public void ShouldNotMapRatingAverage()
        {
            Package package = new Package { RatingAverage = 919 };

            Package mappedPackage = Mapper.Map<Package, Package>(package);

            Assert.AreNotEqual(package.RatingAverage, mappedPackage.RatingAverage, "RatingAverage should not have been mapped.");
        }
    }
}