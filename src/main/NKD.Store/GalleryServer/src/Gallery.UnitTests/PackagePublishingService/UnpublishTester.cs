using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackagePublishingService
{
    [TestFixture]
    public class UnpublishTester : PackagePublishingServiceTester
    {
        [Test]
        public void ShouldCallUnpublish()
        {
            const string key = "key";
            const string packageId = "id";
            const string packageVersion = "version";

            PackagePublishingService.Unpublish(key, packageId, packageVersion);

            MockedPackageUnpublisher.Verify(pp => pp.UnpublishPackage(key, packageId, packageVersion), Times.Once(),
                "Package publisher's Unpublish method should have been invoked.");
        }

        [Test]
        public void ShouldThrowWebFaultExceptionWhenUnpublishThrows()
        {
            var expectedException = new WebFaultException<string>(null, HttpStatusCode.Accepted);
            MockedPackageUnpublisher.Setup(pp => pp.UnpublishPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception());
            MockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(It.IsAny<Exception>(), It.IsAny<string>())).Returns(expectedException);

            TestDelegate methodThatShouldThrow = () => PackagePublishingService.Unpublish(null, null, null);

            ExceptionAssert.Throws(methodThatShouldThrow, expectedException, "Incorrect exception thrown.");
        }
    }
}