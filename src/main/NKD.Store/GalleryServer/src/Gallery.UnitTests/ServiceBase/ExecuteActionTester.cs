using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Infrastructure.Interfaces;
using Gallery.UnitTests.TestHelpers;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Gallery.UnitTests.ServiceBase
{
    [TestFixture]
    public class ExecuteActionTester
    {
        private FakeServiceBase _serviceBase;
        private Mock<IWebFaultExceptionCreator> _mockedWebFaultExceptionFactory;
        private Mock<ILogger> _mockedLogger;

        [SetUp]
        public void SetUp()
        {
            _mockedWebFaultExceptionFactory = new Mock<IWebFaultExceptionCreator>();
            _mockedLogger = new Mock<ILogger>();
            _serviceBase = new FakeServiceBase(_mockedWebFaultExceptionFactory.Object, _mockedLogger.Object);
        }

        [Test]
        public void ShouldThrowExceptionFromFactoryWhenRegularExceptionIsThrownByActionToExecute()
        {
            var exceptionThrown = new Exception();
            const HttpStatusCode expectedStatusCode = HttpStatusCode.InternalServerError;
            Func<int> actionThatThrowsException = () => { throw exceptionThrown; };
            _mockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(exceptionThrown, It.IsAny<string>()))
                .Returns(new WebFaultException<string>(null, expectedStatusCode));

            TestDelegate methodThatShouldThrow = () => _serviceBase.ExecuteAction(actionThatThrowsException);

            ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow, expectedStatusCode);
        }

        [Test]
        public void ShouldLogErrorWhenExceptionThrown()
        {
            Func<int> actionThatThrowsException = () => { throw new Exception(); };
            _mockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(It.IsAny<Exception>(), It.IsAny<string>()))
                .Returns(new WebFaultException<string>(null, HttpStatusCode.InternalServerError));

            try
            {
                _serviceBase.ExecuteAction(actionThatThrowsException);
            }
            catch (WebFaultException<string>)
            {}

            _mockedLogger.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once(),
                "An error should have been logged.");
        }

        [Test]
        public void ShouldNotThrowWhenActionToExecuteDoesNotThrow()
        {
            Func<int> actionThatDoesNotThrow = () => 0;

            TestDelegate methodThatShouldNotThrow = () => _serviceBase.ExecuteAction(actionThatDoesNotThrow);

            Assert.DoesNotThrow(methodThatShouldNotThrow, "No exception should have been thrown.");
        }
    }
}