using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Plugins.NuPackPackageFactory;
using Moq;
using NuGet;
using NUnit.Framework;
using IZipPackage = Gallery.Plugins.NuPackPackageFactory.IZipPackage;

namespace Gallery.Plugins.UnitTests.ZipPackageMapper
{
    [TestFixture]
    public class MapTester
    {
        private IPackageMapper<IZipPackage> _zipPackageMapper;

        private Mock<IZipPackage> _mockedZipPackage;
        private Mock<IDependencyMapper> _mockedDependencyMapper;

        [SetUp]
        public void SetUp()
        {
            _mockedZipPackage = new Mock<IZipPackage>();
            _mockedDependencyMapper = new Mock<IDependencyMapper>();

            _zipPackageMapper = new Plugins.NuPackPackageFactory.ZipPackageMapper(_mockedDependencyMapper.Object);
        }

        [Test]
        public void ShouldSetIdToZipPackageId()
        {
            const string expectedId = "NUnit";
            _mockedZipPackage.SetupGet(zp => zp.Id).Returns(expectedId);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedId, package.Id, "ID was not mapped properly.");
        }

        [Test]
        public void ShouldSetTitleToZipPackageTitle()
        {
            const string expectedTitle = "Moq";
            _mockedZipPackage.SetupGet(zp => zp.Title).Returns(expectedTitle);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedTitle, package.Title, "Title was not mapped properly.");
        }

        [Test]
        public void ShouldSetTitleToIdIfNoTitleInZipPackage()
        {
            const string expectedTitle = "Moq";
            _mockedZipPackage.SetupGet(zp => zp.Id).Returns(expectedTitle);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedTitle, package.Title, "Title was not defaulted to Id");
        }

        [Test]
        public void ShouldSetAuthorsToFlattenedListReturnedByZipPackage()
        {
            IEnumerable<string> authors = new[] { "Bob", "Mary", "Sue" };
            string expectedAuthors = string.Join(", ", authors);
            _mockedZipPackage.SetupGet(zp => zp.Authors).Returns(authors);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedAuthors, package.Authors, "Authors was not mapped properly.");
        }

        [Test]
        public void ShouldSetAuthorsToEmptyStringWhenNullAuthorCollectionReturnedFromZipPackage()
        {
            const IEnumerable<string> nullAuthorCollection = null;
            string expectedAuthors = string.Empty;
            _mockedZipPackage.SetupGet(zp => zp.Authors).Returns(nullAuthorCollection);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedAuthors, package.Authors, "Authors should be mapped to empty string when null Author collection given from ZipPackage.");
        }

        [Test]
        public void ShouldSetVersionToStringOfZipPackageVersion()
        {
            Version expectedVersion = new Version("5.1.7.3");
            _mockedZipPackage.SetupGet(zp => zp.Version).Returns(expectedVersion);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedVersion.ToString(), package.Version, "Version was not mapped properly.");
        }

        [Test]
        public void ShouldSetVersionToEmptyStringWhenNullVersionReturnedFromZipPackage()
        {
            const Version nullVersion = null;
            string expectedVersion = string.Empty;
            _mockedZipPackage.SetupGet(zp => zp.Version).Returns(nullVersion);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedVersion, package.Version, "Version should be mapped to empty string when null Version given from ZipPackage.");
        }

        [Test]
        public void ShouldSetDescriptionToZipPackageDescription()
        {
            const string expectedDescription = "This is a description of a Package.";
            _mockedZipPackage.SetupGet(zp => zp.Description).Returns(expectedDescription);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedDescription, package.Description, "Description was not mapped properly.");
        }

        [Test]
        public void ShouldSetLicenseUrlToStringOfZipPackageLicenseUrl()
        {
            Uri expectedLicenseUrl = new Uri("http://expected.Url/");
            _mockedZipPackage.SetupGet(zp => zp.LicenseUrl).Returns(expectedLicenseUrl);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedLicenseUrl.ToString(), package.LicenseUrl, "LicenseUrl was not mapped properly.");
        }

        [Test]
        public void ShouldSetLicenseUrlToEmptyStringWhenNullLicenseUrlReturnedFromZipPackage()
        {
            const Uri nullUrl = null;
            string expectedUrl = string.Empty;
            _mockedZipPackage.SetupGet(zp => zp.LicenseUrl).Returns(nullUrl);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedUrl, package.LicenseUrl, "LicenseUrl should be mapped to empty string when null LicenseUrl given from ZipPackage.");
        }

        [Test]
        public void ShouldSetLanguageToZipPackageLanguage()
        {
            const string expectedLanguage = "Expected Language";
            _mockedZipPackage.SetupGet(zp => zp.Language).Returns(expectedLanguage);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedLanguage, package.Language, "Language was not mapped properly.");
        }

        [Test]
        public void ShouldSetSummaryToZipPackageSummary()
        {
            const string expectedSummary = "Expected summary";
            _mockedZipPackage.SetupGet(zp => zp.Summary).Returns(expectedSummary);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedSummary, package.Summary, "Summary was not mapped properly.");
        }

        [Test]
        public void ShouldSetIconUrlToZipPackageIconUrl()
        {
            const string expectedIconUrl = "http://url/";
            _mockedZipPackage.SetupGet(zp => zp.IconUrl).Returns(new Uri(expectedIconUrl));

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedIconUrl, package.IconUrl, "IconUrl was not mapped properly.");
        }

        [Test]
        public void ShouldSetIconUrlToEmptyStringWhenNullIconUrlReturnedFromZipPackage()
        {
            const Uri nullUrl = null;
            string expectedUrl = string.Empty;
            _mockedZipPackage.SetupGet(zp => zp.IconUrl).Returns(nullUrl);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedUrl, package.LicenseUrl, "IconUrl should be mapped to empty string when null IconUrl given from ZipPackage.");
        }

        [Test]
        public void ShouldSetProjectUrlToZipPackageProjectUrl()
        {
            const string expectedProjectUrl = "http://url/";
            _mockedZipPackage.SetupGet(zp => zp.ProjectUrl).Returns(new Uri(expectedProjectUrl));

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedProjectUrl, package.ProjectUrl, "ProjectUrl was not mapped properly.");
        }

        [Test]
        public void ShouldSetProjectUrlToEmptyStringWhenNullProjectUrlReturnedFromZipPackage()
        {
            const Uri nullUrl = null;
            string expectedUrl = string.Empty;
            _mockedZipPackage.SetupGet(zp => zp.ProjectUrl).Returns(nullUrl);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedUrl, package.LicenseUrl, "ProjectUrl should be mapped to empty string when null ProjectUrl given from ZipPackage.");
        }

        [Test]
        public void ShouldRequireLicenseAcceptanceToZipPackageRequireLicenseAcceptance()
        {
            const bool expectedRequireLicenseAcceptance = true;
            _mockedZipPackage.SetupGet(zp => zp.RequireLicenseAcceptance).Returns(expectedRequireLicenseAcceptance);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedRequireLicenseAcceptance, package.RequireLicenseAcceptance, "RequireLicenseAcceptance was not mapped properly.");
        }

        [Test]
        public void ShouldCallDependencyMapperPassingPackageDependenciesIn()
        {
            const string packageId = "Package Id";
            Version packageVersion = new Version("1.1.1.1");
            var packageDependencies = new[] {new PackageDependency("id"), new PackageDependency("2"), };
            _mockedZipPackage.SetupGet(zp => zp.Id).Returns(packageId);
            _mockedZipPackage.SetupGet(zp => zp.Version).Returns(packageVersion);
            _mockedZipPackage.SetupGet(zp => zp.Dependencies).Returns(packageDependencies);

            _zipPackageMapper.Map(_mockedZipPackage.Object);

            _mockedDependencyMapper.Verify(dm => dm.Map(packageDependencies, packageId, packageVersion.ToString()), Times.Once(),
                "Package Dependency Mapper was not invoked or was not invoked with the correct arguments.");
        }

        [Test]
        public void ShouldSetDependenciesToWhatDependencyMapperReturns()
        {
            const string expectedDependentPackageId = "nhibernate";
            var expectedDependencies = new List<Dependency>();
            _mockedZipPackage.SetupGet(zp => zp.Id).Returns(expectedDependentPackageId);
            _mockedDependencyMapper.Setup(dm => dm.Map(It.IsAny<IEnumerable<PackageDependency>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedDependencies);

            Package package = _zipPackageMapper.Map(_mockedZipPackage.Object);

            Assert.AreEqual(expectedDependencies, package.Dependencies, "Dependencies did not come from DependencyMapper.");
        }
    }
}