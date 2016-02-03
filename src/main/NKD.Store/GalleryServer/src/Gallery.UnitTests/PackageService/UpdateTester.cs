using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageService
{
    public class UpdateTester : PackageServiceTester
    {
        [Test]
        public void ShouldEnsureGivenKeyHasAccessToPackage() {
            string key = Guid.NewGuid().ToString();
            const string id = "id";
            const string version = "version";
            var packageToUpdate = new Package();

            PackageService.Update(key, id, version, packageToUpdate);

            MockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackage(key, id, version), Times.Once(),
                "PackageAuthenticator's EnsureKeyCanAccessPackage should have been invoked with the specified arguments.");
        }

        [Test]
        public void ShouldThrowWhenAuthenticatorThrows() {
            string key = Guid.NewGuid().ToString();
            const string id = "id";
            const string version = "version";
            var packageToUpdate = new Package();

            MockedPackageAuthenticator.Setup(pa => pa.EnsureKeyCanAccessPackage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new WebFaultException(HttpStatusCode.BadRequest));

            TestDelegate methodThatShouldThrow = () => PackageService.Update(key, id, version, packageToUpdate);

            Assert.Throws<WebFaultException<string>>(methodThatShouldThrow, "Exception should have been thrown since PackageAuthenticator threw.");
        }

        [Test]
        public void ShouldCallPackageUpdater()
        {
            const string key = "key";
            const string packageId = "id";
            const string packageVersion = "version";
            var packageToUpdate = new Package();

            PackageService.Update(key, packageId, packageVersion, packageToUpdate);

            MockedPackageUpdater.Verify(pu => pu.UpdateExistingPackage(packageToUpdate), Times.Once(),
                "Package updater should have been invoked.");
        }

        [Test]
        public void ShouldThrowWebFaultExceptionWhenUpdaterThrows()
        {
            MockedPackageUpdater.Setup(pu => pu.UpdateExistingPackage(It.IsAny<Package>()))
                .Throws(new Exception());

            TestDelegate methodThatShouldThrow = () => PackageService.Update(null, null, null, null);

            Assert.Throws<WebFaultException<string>>(methodThatShouldThrow, "Incorrect type of exception thrown.");
        }
    }
}