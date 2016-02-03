using System;
using System.Net;
using System.ServiceModel.Web;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageService
{
    public class DeleteTester : PackageServiceTester
    {
        [Test]
        public void ShouldEnsureGivenKeyHasAccessToPackage()
        {
            string key = Guid.NewGuid().ToString();
            const string id = "id";
            const string version = "version";

            PackageService.Delete(key, id, version);

            MockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackage(key, id, version), Times.Once(),
                "PackageAuthenticator's EnsureKeyCanAccessPackage should have been invoked with the specified arguments.");
        }

        [Test]
        public void ShouldThrowWhenAuthenticatorThrows()
        {
            MockedPackageAuthenticator.Setup(pa => pa.EnsureKeyCanAccessPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebFaultException(HttpStatusCode.BadRequest));

            TestDelegate methodThatShouldThrow = () => PackageService.Delete("key", "id", "version");

            Assert.Throws<WebFaultException<string>>(methodThatShouldThrow, "Exception should have been thrown since PackageAuthenticator threw.");
        }

        [Test]
        public void ShouldCallDeletePackageInPackageDeleter()
        {
            const string packageId = "Foo";
            const string packageVersion = "Bar";

            PackageService.Delete("", packageId, packageVersion);

            MockedPackageDeleter.Verify(pd => pd.DeletePackage(packageId, packageVersion), Times.Once(), "PackageDeleter should have been called.");
        }
    }
}