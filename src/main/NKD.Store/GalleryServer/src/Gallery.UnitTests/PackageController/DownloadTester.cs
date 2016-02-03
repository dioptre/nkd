using System.IO;
using System.Web.Mvc;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.PackageController
{
    [TestFixture]
    public class DownloadTester
    {
        private Server.Controllers.PackageController _packageController;
        private Mock<IPackageDownloadCountIncrementer> _mockedIncrementer;
        private Mock<IPackageFileGetter> _mockedPackageFileGetter;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;

        [SetUp]
        public void SetUp()
        {
            _mockedIncrementer = new Mock<IPackageDownloadCountIncrementer>();
            _mockedPackageFileGetter = new Mock<IPackageFileGetter>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();

            _packageController = new Server.Controllers.PackageController(_mockedIncrementer.Object, _mockedPackageFileGetter.Object,
                _mockedPublishedPackageRepository.Object, new FakeTaskScheduler());
        }

        [Test]
        public void ShouldThrowWhenPublishedPackageWithGivenIdAndVersionDoesNotExist()
        {
            IQueryable<PublishedPackage> publishedPackages = new PublishedPackage[0].AsQueryable();
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages);

            TestDelegate methodThatShouldThrow = () => _packageController.Download("Id", "Version");

            Assert.Throws<PackageFileDoesNotExistException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldIncrementDownloadCountForPackageHostedByServer()
        {
            PublishedPackage internalPackage = new PublishedPackage {Id = "Id", Version = "Version", ExternalPackageUrl = null};
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[] { internalPackage}.AsQueryable());
            _mockedPackageFileGetter.Setup(pfg => pfg.GetPackageStream(It.IsAny<string>(), It.IsAny<string>())).Returns(Stream.Null);

            _packageController.Download(internalPackage.Id, internalPackage.Version);

            _mockedIncrementer.Verify(i => i.Increment(internalPackage.Id, internalPackage.Version), Times.Once(),
                "Incrementer should have been called exactly once.");
        }

        [Test]
        public void ShouldIncrementDownloadCountForPackageHostedExternally()
        {
            PublishedPackage externalPackage = new PublishedPackage { Id = "Id", Version = "Version", ExternalPackageUrl = "http://foo.com/" };
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[] { externalPackage }.AsQueryable());

            _packageController.Download(externalPackage.Id, externalPackage.Version);

            _mockedIncrementer.Verify(i => i.Increment(externalPackage.Id, externalPackage.Version), Times.Once(),
                "Incrementer should have been called exactly once.");
        }

        [Test]
        public void ShouldReturnFileStreamWhenPackageIsHostedOnServer()
        {
            Stream expectedStream = Stream.Null;
            PublishedPackage internalPackage = new PublishedPackage { Id = "Id", Version = "Version", ExternalPackageUrl = null };
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[] { internalPackage }.AsQueryable());
            _mockedPackageFileGetter.Setup(pfg => pfg.GetPackageStream(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedStream);

            ActionResult actionResult = _packageController.Download(internalPackage.Id, internalPackage.Version);

            Assert.IsInstanceOf<FileStreamResult>(actionResult, "FileStreamResult should have been returned.");
            Assert.AreEqual(expectedStream, ((FileStreamResult)actionResult).FileStream, "Expected FileStream was not returned in FileStreamResult.");
        }

        [Test]
        public void ShouldRedirectToExpectedUrlWhenPackageIsHostedExternally()
        {
            PublishedPackage externalPackage = new PublishedPackage { Id = "Id", Version = "Version", ExternalPackageUrl = "http://foo.com/" };
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(new[] { externalPackage }.AsQueryable());

            ActionResult actionResult = _packageController.Download(externalPackage.Id, externalPackage.Version);

            Assert.IsInstanceOf<RedirectResult>(actionResult, "RedirectResult should have been returned.");
            Assert.AreEqual(externalPackage.ExternalPackageUrl, ((RedirectResult)actionResult).Url,
                "Expected redirect URL was not returned in RedirectResult.");
        }
    }
}