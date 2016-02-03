using System;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageUriValidator
{
    [TestFixture]
    public class ValidatePackageUrisTester
    {
        private IPackageUriValidator _packageUriValidator;

        private Mock<IGalleryUriValidator> _mockedPackageUriValidator;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageUriValidator = new Mock<IGalleryUriValidator>();

            _packageUriValidator = new Core.Impl.PackageUriValidator(_mockedPackageUriValidator.Object);

            _mockedPackageUriValidator.Setup(puv => puv.IsValidUri(It.IsAny<string>(), It.IsAny<UriKind>())).Returns(true);
        }

        private static Package GetPackage()
        {
            return new Package
            {
                Id = "PackageId",
                LicenseUrl = "http://foo.com/license",
                ProjectUrl = "http://foo.com",
                ExternalPackageUrl = "http://foo.com/package.nupkg",
                IconUrl = "http://foo.com/images/icon/png"
            };
        }

        [Test]
        public void ShouldThrowWhenGivenNullPackage()
        {
            Package nullPackage = null;

            TestDelegate methodThatShouldThrow = () => _packageUriValidator.ValidatePackageUris(nullPackage);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldCallUriValdiatorForEachUriProperty()
        {
            var submittedPackage = GetPackage();

            _packageUriValidator.ValidatePackageUris(submittedPackage);

            _mockedPackageUriValidator.Verify(puv => puv.IsValidUri(submittedPackage.LicenseUrl, UriKind.Absolute), Times.Once());
            _mockedPackageUriValidator.Verify(puv => puv.IsValidUri(submittedPackage.ProjectUrl, UriKind.Absolute), Times.Once());
            _mockedPackageUriValidator.Verify(puv => puv.IsValidUri(submittedPackage.ExternalPackageUrl, UriKind.Absolute), Times.Once());
// ReSharper disable RedundantArgumentDefaultValue
            _mockedPackageUriValidator.Verify(puv => puv.IsValidUri(submittedPackage.IconUrl, UriKind.RelativeOrAbsolute), Times.Once());
// ReSharper restore RedundantArgumentDefaultValue
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidLicenseUri()
        {
            var submittedPackage = GetPackage();
            _mockedPackageUriValidator.Setup(puv => puv.IsValidUri(submittedPackage.LicenseUrl, It.IsAny<UriKind>())).Returns(false);

            TestDelegate methodThatShouldThrow = () => _packageUriValidator.ValidatePackageUris(submittedPackage);

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidProjectUri()
        {
            var submittedPackage = GetPackage();
            _mockedPackageUriValidator.Setup(puv => puv.IsValidUri(submittedPackage.ProjectUrl, It.IsAny<UriKind>())).Returns(false);

            TestDelegate methodThatShouldThrow = () => _packageUriValidator.ValidatePackageUris(submittedPackage);

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidExternalPackageUri()
        {
            var submittedPackage = GetPackage();
            _mockedPackageUriValidator.Setup(puv => puv.IsValidUri(submittedPackage.ExternalPackageUrl, It.IsAny<UriKind>())).Returns(false);

            TestDelegate methodThatShouldThrow = () => _packageUriValidator.ValidatePackageUris(submittedPackage);

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldThrowWhenGivenInvalidIconUri()
        {
            var submittedPackage = GetPackage();
            _mockedPackageUriValidator.Setup(puv => puv.IsValidUri(submittedPackage.IconUrl, It.IsAny<UriKind>())).Returns(false);

            TestDelegate methodThatShouldThrow = () => _packageUriValidator.ValidatePackageUris(submittedPackage);

            Assert.Throws<UriFormatException>(methodThatShouldThrow);
        }
    }
}