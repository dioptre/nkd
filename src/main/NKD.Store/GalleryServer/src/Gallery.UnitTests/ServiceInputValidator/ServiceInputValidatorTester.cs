using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.ServiceInputValidator
{
    [TestFixture]
    public class ServiceInputValidatorTester
    {
        private IServiceInputValidator _validator;

        private Mock<IUserKeyValidator> _userKeyValidator;
        private Mock<IPackageIdValidator> _packageIdValidator;
        private Mock<IPackageVersionValidator> _packageVersionValidator;

        const string VALID_USER_KEY = "validKey";
        const string VALID_PACKAGE_ID = "validPackageId";
        const string VALID_PACKAGE_VERSION = "validPackageVersion";

        const string INVALID_USER_KEY = "invalidKey";
        const string INVALID_PACKAGE_ID = "invalidPackageId";
        const string INVALID_PACKAGE_VERSION = "invalidPackageVersion";

        [SetUp]
        public void SetUp()
        {
            _userKeyValidator = new Mock<IUserKeyValidator>();
            _packageIdValidator = new Mock<IPackageIdValidator>();
            _packageVersionValidator = new Mock<IPackageVersionValidator>();
            _validator = new Infrastructure.Impl.ServiceInputValidator(_userKeyValidator.Object, _packageIdValidator.Object, _packageVersionValidator.Object);
        }

        [Test]
        public void ValidateUserApiKeyShouldCallUserKeyValidator()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(INVALID_USER_KEY)).Returns(true);
            _validator.ValidateUserApiKey(INVALID_USER_KEY);
            _userKeyValidator.Verify(ukv => ukv.IsValidUserKey(INVALID_USER_KEY), Times.Once());
        }

        [Test]
        public void ValidateUserApiKeyShouldNotThrowWhenGivenValidKey()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(VALID_USER_KEY)).Returns(true);
            Assert.DoesNotThrow(() => _validator.ValidateUserApiKey(VALID_USER_KEY));
        }

        [Test]
        public void ValidateUserApiKeyShouldThrowWhenGivenInvalidKey()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(INVALID_USER_KEY)).Returns(false);
            Assert.Throws<InvalidUserKeyException>(() => _validator.ValidateUserApiKey(INVALID_USER_KEY));
        }

        [TestCase("invalidUri")]
        [TestCase("http://")]
        [TestCase("bogus.exe")]
        public void ValidateExternalUrlShouldThrowWhenGivenInvalidUri(string uri)
        {
            Assert.Throws<WebFaultException<string>>(() => _validator.ValidateExternalUrl(uri));
        }

        [TestCase(@"mailto:foo@bar.com")]
        [TestCase(@"file://foo.exe")]
        [TestCase(@"C:\Windows\bar.exe")]
        public void ValidateExternalUrlShouldThrowWhenGivenUriOtherThanHttpHttpsOrFtp(string uri)
        {
            Assert.Throws<WebFaultException<string>>(() => _validator.ValidateExternalUrl(uri));
        }

        [TestCase(@"http://example.com/myPackage.nupkg")]
        [TestCase(@"https://example.com/otherPackage.nupkg")]
        [TestCase(@"ftp://ftp.example.com/myPackage.nupkg")]
        public void ValidateExternalUrlShouldNotThrowWhenGivenValidHttpHttpsOrFtpUrl(string uri)
        {
            Assert.DoesNotThrow(() => _validator.ValidateExternalUrl(uri));
        }

        [Test]
        public void ValidateAllPackageKeysShouldValidateUserKeyPackageIdAndPackageVersion()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(VALID_USER_KEY)).Returns(true);
            _packageIdValidator.Setup(piv => piv.IsValidPackageId(VALID_PACKAGE_ID)).Returns(true);
            _packageVersionValidator.Setup(pvv => pvv.IsValidPackageVersion(VALID_PACKAGE_VERSION)).Returns(true);

            _validator.ValidateAllPackageKeys(VALID_USER_KEY, VALID_PACKAGE_ID, VALID_PACKAGE_VERSION);

            _userKeyValidator.Verify(ukv => ukv.IsValidUserKey(VALID_USER_KEY), Times.Once());
            _packageIdValidator.Verify(piv => piv.IsValidPackageId(VALID_PACKAGE_ID), Times.Once());
            _packageVersionValidator.Verify(pvv => pvv.IsValidPackageVersion(VALID_PACKAGE_VERSION), Times.Once());
        }

        [Test]
        public void ValidateAllPackageKeysShouldThrowWhenGivenInvalidKey()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(INVALID_USER_KEY)).Returns(false);
            _packageIdValidator.Setup(piv => piv.IsValidPackageId(VALID_PACKAGE_ID)).Returns(true);
            _packageVersionValidator.Setup(pvv => pvv.IsValidPackageVersion(VALID_PACKAGE_VERSION)).Returns(true);

            Assert.Throws<InvalidUserKeyException>(() => _validator.ValidateAllPackageKeys(INVALID_USER_KEY, VALID_PACKAGE_ID, VALID_PACKAGE_VERSION));
        }

        [Test]
        public void ValidateAllPackageKeysShouldThrowWhenGivenInvalidPackageId()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(VALID_USER_KEY)).Returns(true);
            _packageIdValidator.Setup(piv => piv.IsValidPackageId(INVALID_PACKAGE_ID)).Returns(false);
            _packageVersionValidator.Setup(pvv => pvv.IsValidPackageVersion(VALID_PACKAGE_VERSION)).Returns(true);

            Assert.Throws<InvalidPackageIdException>(() => _validator.ValidateAllPackageKeys(VALID_USER_KEY, INVALID_PACKAGE_ID, VALID_PACKAGE_VERSION));
        }

        [Test]
        public void ValidateAllPackageKeysShouldThrowWhenGivenInvalidPackageVersion()
        {
            _userKeyValidator.Setup(ukv => ukv.IsValidUserKey(VALID_USER_KEY)).Returns(true);
            _packageIdValidator.Setup(piv => piv.IsValidPackageId(VALID_PACKAGE_ID)).Returns(true);
            _packageVersionValidator.Setup(pvv => pvv.IsValidPackageVersion(INVALID_PACKAGE_VERSION)).Returns(false);

            Assert.Throws<InvalidPackageVersionException>(() => _validator.ValidateAllPackageKeys(VALID_USER_KEY, VALID_PACKAGE_ID, INVALID_PACKAGE_VERSION));
        }

        [Test]
        public void ValidateKeysMatchInstanceShouldThrowWhenPackageIdsDoNotMatch()
        {
            Package packageInstance = new Package {Id = VALID_PACKAGE_ID + "different", Version = VALID_PACKAGE_VERSION};

            TestDelegate methodThatShouldThrow = () => _validator.ValidateKeysMatchInstance(VALID_PACKAGE_ID, VALID_PACKAGE_VERSION, packageInstance);

            Assert.Throws<InvalidPackageIdException>(methodThatShouldThrow, "Nonmatching PackageIds should throw correct exception.");
        }

        [Test]
        public void ValidateKeysMatchInstanceShouldThrowWhenPackageVersionsDoNotMatch()
        {
            Package packageInstance = new Package { Id = VALID_PACKAGE_ID, Version = VALID_PACKAGE_VERSION + ".1" };

            TestDelegate methodThatShouldThrow = () => _validator.ValidateKeysMatchInstance(VALID_PACKAGE_ID, VALID_PACKAGE_VERSION, packageInstance);

            Assert.Throws<InvalidPackageVersionException>(methodThatShouldThrow, "Nonmatching PackageVersions should throw correct exception.");
        }

        [Test]
        public void ValidateKeysMatchInstanceShouldNotThrowWhenPackageIdsAndVersionsMatch()
        {
            Package packageInstance = new Package { Id = VALID_PACKAGE_ID, Version = VALID_PACKAGE_VERSION };

            TestDelegate methodThatShouldNotThrow = () => _validator.ValidateKeysMatchInstance(VALID_PACKAGE_ID, VALID_PACKAGE_VERSION, packageInstance);

            Assert.DoesNotThrow(methodThatShouldNotThrow, "Matching PackageIds and PackageVersions should not throw an exception.");
        }
    }
}