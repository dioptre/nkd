using System;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageSlugCreator
{
    [TestFixture]
    public class CreateSlugTester
    {
        private readonly IPackageSlugCreator _packageSlugCreator = new Core.Impl.PackageSlugCreator();

        [Test]
        public void ShouldReplaceDotsWithDashesInPackageIdAndVersion()
        {
            const string packageId = "My.Great.Package";
            const string packageVersion = "1.3.5";
            const string expectedSlug = "My-Great-Package-1-3-5";

            string slug = _packageSlugCreator.CreateSlug(packageId, packageVersion);

            Assert.AreEqual(expectedSlug, slug, "Dots were not replaced with dashes.");
        }

        [Test]
        public void ShouldReplaceSpacesWithDashesInPackageId()
        {
            const string packageId = "My Great Package";
            const string packageVersion = "1.3.5";
            const string expectedSlug = "My-Great-Package-1-3-5";

            string slug = _packageSlugCreator.CreateSlug(packageId, packageVersion);

            Assert.AreEqual(expectedSlug, slug, "Spaces were not replaced with dashes.");
        }

        [Test]
        public void ShouldJoinIdAndVersionWithDash()
        {
            const string packageId = "pack";
            const string packageVersion = "1";
            const string expectedSlug = packageId + "-" + packageVersion;

            string slug = _packageSlugCreator.CreateSlug(packageId, packageVersion);

            Assert.AreEqual(expectedSlug, slug, "ID and Version should be concatenated with a dash.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpacePackageId(string packageId)
        {
            const string packageVersion = "1.0";

            TestDelegate methodThatShouldThrow = () => _packageSlugCreator.CreateSlug(packageId, packageVersion);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "ArgumentNullException should have been thrown.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpacePackageVersion(string packageVersion)
        {
            const string packageId = "Package-Id";

            TestDelegate methodThatShouldThrow = () => _packageSlugCreator.CreateSlug(packageId, packageVersion);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "ArgumentNullException should have been thrown.");
        }
    }
}