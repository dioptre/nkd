using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageVersionValidator
{
    [TestFixture]
    public class IsValidPackageVersionTester
    {
        private readonly IPackageVersionValidator _validator = new Core.Impl.PackageVersionValidator();

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void ShouldNotAllowNullOrEmptyStrings(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("abc")]
        [TestCase("A")]
        [TestCase("a.b.c")]
        [TestCase("1.2.c")]
        public void ShouldNotAllowVersionsContainingLetters(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("1")]
        [TestCase("10")]
        public void ShouldNotAllowNumberOnly(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("1.")]
        [TestCase("10.")]
        public void ShouldNotAllowNumberDot(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("1.0")]
        [TestCase("10.2")]
        public void ShouldAllowNumberDotNumber(string version)
        {
            Assert.IsTrue(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should validate.", version);
        }

        [TestCase("1.0.")]
        [TestCase("10.2.")]
        public void ShouldNotAllowNumberDotNumberDot(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("1.0.1")]
        [TestCase("10.2.3")]
        public void ShouldAllowNumberDotNumberDotNumber(string version)
        {
            Assert.IsTrue(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should validate.", version);
        }

        [TestCase("1.0.2.")]
        [TestCase("10.2.3.")]
        public void ShouldNotAllowNumberDotNumberDotNumberDot(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("1.0.1.2")]
        [TestCase("10.2.3.4")]
        public void ShouldAllowNumberDotNumberDotNumberDotNumber(string version)
        {
            Assert.IsTrue(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should validate.", version);
        }

        [TestCase("..")]
        [TestCase("1..")]
        [TestCase("..1")]
        [TestCase("1..2")]
        [TestCase("1.2..")]
        [TestCase("1.2.3..")]
        public void ShouldNotAllowAnyVersionWithDotDot(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }

        [TestCase("2.0.0.໘")]
        [TestCase("໘.໘.໘.໘")]
        public void ShouldNotAllowForeignNumbers(string version)
        {
            Assert.IsFalse(_validator.IsValidPackageVersion(version), "PackageVersion of {0} should not validate.", version);
        }
    }
}