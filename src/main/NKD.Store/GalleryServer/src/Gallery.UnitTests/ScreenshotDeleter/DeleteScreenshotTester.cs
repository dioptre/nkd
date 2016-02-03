using System;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.ScreenshotDeleter
{
    [TestFixture]
    public class DeleteScreenshotTester
    {
        private Mock<IRepository<Screenshot>> _mockedScreenshotRepository;
        private Mock<IRepository<PublishedScreenshot>> _mockedPublishedScreenshotRepository;
        private Mock<IMapper> _mockedMapper;

        private IScreenshotDeleter _screenshotDeleter;

        [SetUp]
        public void SetUp()
        {
            _mockedScreenshotRepository = new Mock<IRepository<Screenshot>>();
            _mockedPublishedScreenshotRepository = new Mock<IRepository<PublishedScreenshot>>();
            _mockedMapper = new Mock<IMapper>();

            _screenshotDeleter = new Infrastructure.Impl.ScreenshotDeleter(_mockedScreenshotRepository.Object, _mockedPublishedScreenshotRepository.Object,
                _mockedMapper.Object, new Mock<IRepository<PublishedPackage>>().Object);
        }

        [Test]
        public void ShouldCallScreenshotRepositoryCollection()
        {
            const int screenshotId = 7;
            _mockedScreenshotRepository.SetupGet(sr => sr.Collection)
                .Returns(new[] { new Screenshot { Id = screenshotId } }.AsQueryable())
                .Verifiable("ScreenshotRepository's Collection should have been retrieved.");

            _screenshotDeleter.DeleteScreenshot(screenshotId);

            _mockedScreenshotRepository.VerifyGet(sr => sr.Collection, Times.AtMost(2));
        }

        [Test]
        public void ShouldCallPublishedScreenshotRepositoryDeleteMany()
        {
            const int screenshotId = 5;
            _mockedScreenshotRepository.SetupGet(sr => sr.Collection).Returns(new[] { new Screenshot { Id = screenshotId } }.AsQueryable());

            _screenshotDeleter.DeleteScreenshot(screenshotId);

            _mockedPublishedScreenshotRepository.Verify(psr => psr.DeleteMany(It.IsAny<Func<PublishedScreenshot, bool>>()), Times.Once(),
                "PublishedScreenshotRepository's DeleteMany should have been called.");
        }
    }
}