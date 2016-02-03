using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Exceptions;
using Microsoft.Http;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageAuthenticator
{
    public class EnsureKeyCanAccessPackageTester : PackageTesterBase
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpaceKey(string key)
        {
            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage(key, "packageId", "packageVersion");

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Should have thrown when given null, empty, or white space key.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpacePackageId(string packageId)
        {
            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", packageId, "packageVersion");

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Should have thrown when given null, empty, or white space or empty package ID.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpacePackageVersion(string packageVersion)
        {
            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", packageVersion);

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Should have thrown when given null, empty, or white space package version.");
        }

        [Test]
        public void ShouldUseFrontEndWebSiteRootForHttpClient()
        {
            const string expectedWebSiteRoot = "web site root";
            MockedConfigSettings.SetupGet(cs => cs.FrontEndWebSiteRoot).Returns(expectedWebSiteRoot);
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(OkHttpResponse);

            PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", "packageVersion");

            MockedHttpClientAdapter.Verify(hca => hca.GetHttpClient(expectedWebSiteRoot), Times.Once(),
                "Should have called HttpClientAdpater's GetHttpClient() with expected web site root.");
        }

        [Test]
        public void ShouldPutValidatePackageKeyUriAndParametersInRequestUri()
        {
            const string expectedValidatePackageKeyUri = "foo";
            const string key = "asdf";
            const string packageId = "packageId";
            const string packageVersion = "packageVersion";
            string expectedUri = string.Format("{0}/{1}/{2}/{3}", expectedValidatePackageKeyUri, key, packageId, packageVersion);

            MockedConfigSettings.SetupGet(cs => cs.ValidatePackageKeyUri).Returns(expectedValidatePackageKeyUri);
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(OkHttpResponse);

            PackageAuthenticator.EnsureKeyCanAccessPackage(key, packageId, packageVersion);

            MockedHttpClient.Verify(hc => hc.Get(expectedUri), Times.Once(),
                string.Format("Should have called HttpClient's Get() with expected URI '{0}'.", expectedUri));
        }

        [Test]
        public void ShouldThrowWhenGivenResponseReturnedBadRequest()
        {
            var response = new HttpResponseMessage {StatusCode = HttpStatusCode.BadRequest};
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(response);

            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", "packageVersion");

            Assert.Throws<Exception>(methodThatShouldThrow, "Should have thrown when reponse returned not-OK status code.");
        }

        [Test]
        public void ShouldThrowWhenReturnedContentIsNotABoolean()
        {
            OkHttpResponse.Content = HttpContent.Create("not a bool");
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(OkHttpResponse);

            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", "packageVersion");

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Should have thrown when reponse returned non-boolean.");
        }

        [Test]
        public void ShouldThrowWhenFalseReturnedFromResponse()
        {
            OkHttpResponse.Content = HttpContent.Create("false");
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(OkHttpResponse);

            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", "packageVersion");

            Assert.Throws<PackageAuthorizationException>(methodThatShouldThrow, "Should have thrown when reponse returned false.");
        }

        [Test]
        public void ShouldNotThrowWhenTrueReturnedFromResponse()
        {
            OkHttpResponse.Content = HttpContent.Create("true");
            MockedHttpClientAdapter.Setup(hca => hca.GetHttpClient(It.IsAny<string>())).Returns(MockedHttpClient.Object);
            MockedHttpClient.Setup(hc => hc.Get(It.IsAny<string>())).Returns(OkHttpResponse);

            TestDelegate methodThatShouldThrow = () => PackageAuthenticator.EnsureKeyCanAccessPackage("key", "packageId", "packageVersion");

            Assert.DoesNotThrow(methodThatShouldThrow, "Should not have thrown when reponse returned true.");
        }
    }
}