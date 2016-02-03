using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.ScreenshotService
{
    [TestFixture]
    public class DeleteTester
    {
        private Mock<IScreenshotDeleter> _mockedScreenshotDeleter;
        private Mock<IRepository<Screenshot>> _mockedScreenshotRepository;
        private Mock<IPackageAuthenticator> _mockedPackageAuthenticator;
        private Mock<IServiceInputValidator> _serviceInputValidator;
        private Server.ScreenshotService _screenshotService;

        [SetUp]
        public void SetUp()
        {
            _mockedScreenshotDeleter = new Mock<IScreenshotDeleter>();
            _mockedScreenshotRepository = new Mock<IRepository<Screenshot>>();
            _mockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            _serviceInputValidator = new Mock<IServiceInputValidator>();

            _screenshotService = new Server.ScreenshotService(_mockedScreenshotDeleter.Object, null, _mockedPackageAuthenticator.Object,
                _mockedScreenshotRepository.Object, _serviceInputValidator.Object, null);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void ShouldCallScreenshotDeleterWhenGivenAnIntegerGreaterThan0(int id)
        {
            _mockedScreenshotRepository.SetupGet(sr => sr.Collection).Returns(new[] { new Screenshot {Id = id} }.AsQueryable());
            _screenshotService.Delete("key", id.ToString());

            _mockedScreenshotDeleter.Verify(sd => sd.DeleteScreenshot(id), Times.Once(),
                "ScreenshotDeleter's DeleteScreenshot should have been invoked with given ID.");
        }

        [TestCase("3.5")]
        [TestCase("string var")]
        public void ShouldThrowWithBadRequestStatusCodeWhenGivenNonInteger(string nonIntegerId)
        {
            TestDelegate methodThatShouldThrow = () => _screenshotService.Delete("key", nonIntegerId);

            ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow, HttpStatusCode.BadRequest);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void ShouldThrowWhenGivenNullOrEmptyString(string nullOrEmptyId)
        {
            TestDelegate methodThatShouldThrow = () => _screenshotService.Delete("key", nullOrEmptyId);

            Assert.Throws<WebFaultException<string>>(methodThatShouldThrow);
        }
    }
}