using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Enums;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackagePublishingService
{
    public class RePublishTester : PackagePublishingServiceTester
    {
        [Test]
        public void RePublishShouldPublishWithRePublishLogAction()
        {
            const string key = "key";
            const string packageId = "id";
            const string packageVersion = "version";

            PackagePublishingService.RePublish(key, packageId, packageVersion);

            MockedPackagePublisher.Verify(pp => pp.PublishPackage(key, packageId, packageVersion, PackageLogAction.RePublish), Times.Once(),
                "Package publisher should have been invoked with the correct PackageLogAction.");
        }

        [Test]
        public void ShouldThrowWebFaultExceptionWhenRePublishThrows()
        {
            var expectedException = new WebFaultException<string>(null, HttpStatusCode.Accepted);
            MockedPackagePublisher.Setup(pp => pp.PublishPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PackageLogAction>()))
                .Throws(new Exception());
            MockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(It.IsAny<Exception>(), It.IsAny<string>())).Returns(expectedException);

            TestDelegate methodThatShouldThrow = () => PackagePublishingService.RePublish(null, null, null);

            ExceptionAssert.Throws(methodThatShouldThrow, expectedException, "Incorrect exception thrown.");
        }
    }
}