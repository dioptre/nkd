using System;
using Gallery.Core.Domain;
using Gallery.Plugins.NuPackPackageFactory;
using Moq;
using NuGet;
using NUnit.Framework;

namespace Gallery.Plugins.UnitTests.DependencyMapper
{
    public class MapTester
    {
        private readonly IDependencyMapper _dependencyMapper = new Plugins.NuPackPackageFactory.DependencyMapper();
        private Mock<IVersionSpec> _mockedVersionSpec;

        [SetUp]
        public void SetUp()
        {
            _mockedVersionSpec = new Mock<IVersionSpec>();
        }

        [Test]
        public void ShouldThrowWhenGivenNullDependency()
        {
            PackageDependency nullDependency = null;

            TestDelegate methodThatShouldThrow = () => _dependencyMapper.Map(nullDependency, "id", "version");

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown for null PackageDependency.");
        }

        [Test]
        public void ShouldSetPackageIdToGivenPackageId()
        {
            const string expectedPackageId = "package id";

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", _mockedVersionSpec.Object), expectedPackageId, "version");

            Assert.AreEqual(expectedPackageId, dependency.PackageId, "PackageId was not mapped properly.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldSetPackageIdEmptyStringWhenGivenNullEmptyOrWhiteSpacePackageId(string packageId)
        {
            string expectedPackageId = string.Empty;

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", _mockedVersionSpec.Object), packageId, "version");

            Assert.AreEqual(expectedPackageId, dependency.PackageId, "Dependency's PackageId should have been set to an empty string.");
        }

        [Test]
        public void ShouldSetPackageVersionToGivenPackageVersion()
        {
            const string expectedPackageVersion = "package version";

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", _mockedVersionSpec.Object), "package id", expectedPackageVersion);

            Assert.AreEqual(expectedPackageVersion, dependency.PackageVersion, "PackageVersion was not mapped properly.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldSetPackageVersionEmptyStringWhenGivenNullEmptyOrWhiteSpacePackageVersion(string packageVersion)
        {
            string expectedPackageVersion = string.Empty;

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", _mockedVersionSpec.Object), "package id", packageVersion);

            Assert.AreEqual(expectedPackageVersion, dependency.PackageVersion, "Dependency's PackageVersion should have been set to an empty string.");
        }

        [Test]
        public void ShouldSetVersionSpecToVersionSpecString()
        {
            const string expectedVersionSpecString = "[1.0,2.0]";
            _mockedVersionSpec.SetupToString().Returns(expectedVersionSpecString);

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", _mockedVersionSpec.Object), "package id", "package version");

            Assert.AreEqual(expectedVersionSpecString, dependency.VersionSpec, "VersionSpec should be set to given Package's VersionSpec string.");
        }

        [Test]
        public void ShouldSetVersionSpecToEmptyStringWhenNullVersionSpecGiven()
        {
            const IVersionSpec nullVersionSpec = null;

            Dependency dependency = _dependencyMapper.Map(new PackageDependency("id", nullVersionSpec), "package id", "package version");

            Assert.IsEmpty(dependency.VersionSpec, "VersionSpec should be set to empty string when null VersionSpec given.");
        }
    }
}