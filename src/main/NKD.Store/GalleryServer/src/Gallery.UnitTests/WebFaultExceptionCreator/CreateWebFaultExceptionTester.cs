using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Exceptions;
using Gallery.Infrastructure.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.WebFaultExceptionCreator
{
    [TestFixture]
    public class CreateWebFaultExceptionTester
    {
        private readonly IWebFaultExceptionCreator _webFaultExceptionCreator = new Infrastructure.Impl.WebFaultExceptionCreator();

        [Test]
        public void ShouldRethrowExceptionInActionWhenItIsAWebFaultExceptionWithString()
        {
            var expectedException = new WebFaultException<string>(string.Empty, HttpStatusCode.BadRequest);

            WebFaultException<string> returnedException = _webFaultExceptionCreator.CreateWebFaultException(expectedException, string.Empty);

            Assert.AreEqual(expectedException, returnedException, "Expected WebFaultException<string> was not rethrown.");
        }

        [Test]
        public void ShouldThrowWithGivenStatusCodeWhenGivenWebFaultException()
        {
            const HttpStatusCode expectedCode = HttpStatusCode.BadGateway;
            WebFaultException originalException = new WebFaultException(expectedCode);

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(originalException, string.Empty);

            Assert.AreEqual(expectedCode, webFaultException.StatusCode, "Incorrect status code returned.");
        }

        [Test]
        public void ShouldThrowExceptionWithStatusCodeNotFoundWhenObjectDoesNotExistExceptionThrown()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            var exception = new ObjectDoesNotExistException();

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(exception, string.Empty);


            Assert.AreEqual(expectedStatusCode, webFaultException.StatusCode, "Incorrect StatusCode returned.");
        }

        [Test]
        public void ShouldThrowExceptionWithStatusCodeUnauthorizedWhenPackageAuthorizationExceptionThrown()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized;
            var exception = new PackageAuthorizationException();

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(exception, string.Empty);

            Assert.AreEqual(expectedStatusCode, webFaultException.StatusCode, "Incorrect StatusCode returned.");
        }

        [Test]
        public void ShouldReturnInternalServerErrorWhenGivenOtherException()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.InternalServerError;
            var originalException = new Exception();

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(originalException, string.Empty);

            Assert.AreEqual(expectedStatusCode, webFaultException.StatusCode, "Incorrect status code returned.");
        }

        [Test]
        public void ShouldReturnExceptionMessageContainingPrefix()
        {
            const string errorMessagePrefix = "message prefix";

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(new Exception(), errorMessagePrefix);

            StringAssert.Contains(errorMessagePrefix, webFaultException.Detail, "Prefix not contained in message.");
        }

        [Test]
        public void ShouldReturnExceptionMessageContainingOriginalExceptionMessage()
        {
            Exception originalException = new Exception("message here");

            WebFaultException<string> webFaultException = _webFaultExceptionCreator.CreateWebFaultException(originalException, string.Empty);

            StringAssert.Contains(originalException.Message, webFaultException.Detail, "Prefix not contained in message.");
        }
    }
}