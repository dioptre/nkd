using System;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageReportAbuseUriCreator
{
    [TestFixture]
    public class CreateUriTester
    {
        private IPackageReportAbuseUriCreator _creator;
        private Mock<IConfigSettings> _mockedConfigSettings;
        private PublishedPackage _testPackage;

        [SetUp]
        public void SetUp()
        {
            _mockedConfigSettings = new Mock<IConfigSettings>();
            _creator = new Infrastructure.Impl.PackageReportAbuseUriCreator(_mockedConfigSettings.Object);
            _testPackage = new PublishedPackage { Id = "TestId", Version = "TestVersion" };
        }

        [Test]
        public void ShouldThrowWhenNoPackageIsGiven()
        {
            PublishedPackage nullPackage = null;

            TestDelegate methodThatShouldThrow = () => _creator.CreateUri(nullPackage);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldReadReportAbuseUriTemplateFromConfigSettings()
        {
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(string.Empty);

            _creator.CreateUri(_testPackage);

            _mockedConfigSettings.VerifyGet(cs => cs.ReportAbuseUriTemplate, Times.Once(), "Should have retrieved ReportAbuseUriTemplate from IConfigSettings.");
        }

        [Test]
        public void ShouldReturnUriWithCorrectPackageIdWhenTemplateContainsPackageIdToken()
        {
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(PackageUriTemplateToken.PackageId);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.Contains(_testPackage.Id, uri, "Should have returned URI with package ID.");
        }

        [Test]
        public void ShouldReturnUriWithoutPackageIdWhenTemplateLacksPackageIdToken()
        {
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(string.Empty);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.DoesNotContain(_testPackage.Id, uri, "PackageId should not have been included in URI.");
        }

        [Test]
        public void ShouldReturnUriContainingPackageVersionWhenTemplateContainsPackageIdToken()
        {
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(PackageUriTemplateToken.PackageVersion);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.Contains(_testPackage.Version, uri, "Should have returned URI with package version.");
        }

        [Test]
        public void ShouldReturnUriWithoutPackageVersionWhenTemplateLacksPackageVersionToken()
        {
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(string.Empty);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.DoesNotContain(_testPackage.Version, uri, "Should have returned URI without package version.");
        }

        [Test]
        public void ShouldReplacePackageIdTokenWithPackageId()
        {
            string template = string.Format("foo/{0}", PackageUriTemplateToken.PackageId);
            string expectedUri = template.Replace(PackageUriTemplateToken.PackageId, _testPackage.Id);
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(template);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.AreEqualIgnoringCase(expectedUri, uri, "PackageId token should have been replaced by the package's ID.");
        }

        [Test]
        public void ShouldReplacePackageVersionTokenWithPackageVersion()
        {
            string template = string.Format("foo/{0}", PackageUriTemplateToken.PackageVersion);
            string expectedUri = template.Replace(PackageUriTemplateToken.PackageVersion, _testPackage.Version);
            _mockedConfigSettings.SetupGet(cs => cs.ReportAbuseUriTemplate).Returns(template);

            string uri = _creator.CreateUri(_testPackage);

            StringAssert.AreEqualIgnoringCase(expectedUri, uri, "PackageVersion token should have been replaced by the package's ID.");
        }
    }
}