using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageService
{
    [TestFixture]
    public abstract class PackageServiceTester
    {
        protected Server.PackageService PackageService;

        protected Mock<IMapper> MockedMapper;
        protected Mock<IRepository<Package>> MockedPackageRepository;
        protected Mock<IRepository<PublishedPackage>> MockedPublishedPackageRepository;
        protected Mock<IWebFaultExceptionCreator> MockedWebFaultExceptionFactory;
        protected Mock<IPackageDeleter> MockedPackageDeleter;
        protected Mock<IPackageAuthenticator> MockedPackageAuthenticator;
        protected Mock<IPackageUpdater> MockedPackageUpdater;
        protected Mock<IServiceInputValidator> MockedInputValidator;
        protected Mock<ILogger> MockedLogger;
        protected Mock<IPackageGetter> MockedPackageGetter;
        protected Mock<IPackageRatingUpdater> MockedPackageRatingUpdater;
        protected Mock<IRatingAuthorizer> MockedRatingAuthorizer;
        protected Mock<IUnfinishedPackageGetter> MockedUnfinishedPackageGetter;

        [SetUp]
        public void SetUpBase()
        {
            MockedPackageRepository = new Mock<IRepository<Package>>();
            MockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            MockedMapper = new Mock<IMapper>();
            MockedPackageDeleter = new Mock<IPackageDeleter>();
            MockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            MockedPackageUpdater = new Mock<IPackageUpdater>();
            MockedWebFaultExceptionFactory = new Mock<IWebFaultExceptionCreator>();
            MockedInputValidator = new Mock<IServiceInputValidator>();
            MockedLogger = new Mock<ILogger>();
            MockedPackageGetter = new Mock<IPackageGetter>();
            MockedPackageRatingUpdater = new Mock<IPackageRatingUpdater>();
            MockedRatingAuthorizer = new Mock<IRatingAuthorizer>();
            MockedUnfinishedPackageGetter = new Mock<IUnfinishedPackageGetter>();

            PackageService = new Server.PackageService(MockedPackageDeleter.Object, MockedPackageAuthenticator.Object,
                MockedPackageUpdater.Object, MockedWebFaultExceptionFactory.Object, MockedInputValidator.Object, MockedLogger.Object, MockedPackageGetter.Object,
                MockedPackageRatingUpdater.Object, MockedRatingAuthorizer.Object, MockedUnfinishedPackageGetter.Object);

            MockedWebFaultExceptionFactory.Setup(wfef => wfef.CreateWebFaultException(It.IsAny<Exception>(), It.IsAny<string>()))
                .Returns(new WebFaultException<string>(null, HttpStatusCode.OK));
        }
    }
}