using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Gallery.UnitTests.PackagePublishingService
{
    [TestFixture]
    public abstract class PackagePublishingServiceTester
    {
        protected Server.PackagePublishingService PackagePublishingService;

        protected Mock<IPackagePublisher> MockedPackagePublisher;
        protected Mock<IPackageUnpublisher> MockedPackageUnpublisher;
        protected Mock<IWebFaultExceptionCreator> MockedWebFaultExceptionFactory;
        protected Mock<IServiceInputValidator> MockedServiceInputValidator;

        [SetUp]
        public void SetUp()
        {
            MockedPackagePublisher = new Mock<IPackagePublisher>();
            MockedPackageUnpublisher = new Mock<IPackageUnpublisher>();
            MockedWebFaultExceptionFactory = new Mock<IWebFaultExceptionCreator>();
            MockedServiceInputValidator = new Mock<IServiceInputValidator>();
            PackagePublishingService = new Server.PackagePublishingService(MockedPackagePublisher.Object, MockedPackageUnpublisher.Object,
                MockedWebFaultExceptionFactory.Object, MockedServiceInputValidator.Object, new Mock<ILogger>().Object);
        }
    }
}