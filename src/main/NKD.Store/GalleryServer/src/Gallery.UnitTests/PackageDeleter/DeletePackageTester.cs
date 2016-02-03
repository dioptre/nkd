using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.PackageDeleter {
    [TestFixture]
    public class DeletePackageTester {
        private IPackageDeleter _packageDeleter;

        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IRepository<Screenshot>> _mockedScreenshotRespository;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;
        private Mock<IRepository<PublishedScreenshot>> _mockedPublishedScreenshotRepository;
        private Mock<IPackageFileGetter> _mockedPackageFileGetter;
        private Mock<IFileSystem> _mockedFileSystem;
        private Mock<IPackageLogEntryCreator> _mockedPackageLogEntryCreator;
        private Mock<IRepository<Dependency>> _mockedDependencyRepository;
        private Mock<IRecommendedVersionManager<Package>> _mockedPackageRecommendedVersionManager;
        private Mock<IRecommendedVersionManager<PublishedPackage>> _mockedPublishedPackageRecommendedVersionManager;
        private Mock<IPackageDataAggregateUpdater> _mockedPackageDataAggregateUpdater;
        private Mock<IRepository<PackageDataAggregate>> _mockedPackageDataAggregateRepo;

        private Package _existingPackage;

        [SetUp]
        public void SetUp() {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedScreenshotRespository = new Mock<IRepository<Screenshot>>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            _mockedPublishedScreenshotRepository = new Mock<IRepository<PublishedScreenshot>>();
            _mockedPackageFileGetter = new Mock<IPackageFileGetter>();
            _mockedFileSystem = new Mock<IFileSystem>();
            _mockedPackageLogEntryCreator = new Mock<IPackageLogEntryCreator>();
            _mockedDependencyRepository = new Mock<IRepository<Dependency>>();
            _mockedPackageRecommendedVersionManager = new Mock<IRecommendedVersionManager<Package>>();
            _mockedPublishedPackageRecommendedVersionManager = new Mock<IRecommendedVersionManager<PublishedPackage>>();
            _mockedPackageDataAggregateUpdater = new Mock<IPackageDataAggregateUpdater>();
            _mockedPackageDataAggregateRepo = new Mock<IRepository<PackageDataAggregate>>();

            _packageDeleter = new Infrastructure.Impl.PackageDeleter(_mockedPackageRepository.Object, _mockedScreenshotRespository.Object,
                _mockedPublishedPackageRepository.Object, _mockedPublishedScreenshotRepository.Object, _mockedPackageFileGetter.Object, _mockedFileSystem.Object,
                _mockedPackageLogEntryCreator.Object, _mockedDependencyRepository.Object, _mockedPackageRecommendedVersionManager.Object,
                _mockedPublishedPackageRecommendedVersionManager.Object, _mockedPackageDataAggregateUpdater.Object, _mockedPackageDataAggregateRepo.Object);

            _existingPackage = new Package { Id = "Id-" + Guid.NewGuid(), Version = "Version-" + Guid.NewGuid() };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[] { _existingPackage }.AsQueryable());
        }

        [Test]
        public void ShouldCreateDeletedPackageRecord() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageLogEntryCreator.Verify(plec => plec.Create(_existingPackage.Id, _existingPackage.Version, PackageLogAction.Delete), Times.Once(),
                "A DeletedPackage record should have been created.");
        }

        [Test]
        public void ShouldCallGetPackagePathInPackageFileGetter() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageFileGetter.Verify(pfg => pfg.GetPackagePath(_existingPackage.Id, _existingPackage.Version), Times.Once(),
                "Should have called PackageFileGetter.GetPackagePath.");
        }

        [Test]
        public void ShouldCallDeleteFileIfItExists() {
            const string packageDirectory = "path";
            _mockedPackageFileGetter.Setup(pfg => pfg.GetPackagePath(It.IsAny<string>(), It.IsAny<string>())).Returns(packageDirectory);

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedFileSystem.Verify(fs => fs.DeleteFileIfItExists(packageDirectory), Times.Once(),
                "DeleteFileIfItExists on IFileSystem should have been called.");
        }

        [Test]
        public void ShouldCallGetPackageDirectoryInPackageFileGetter() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageFileGetter.Verify(pfg => pfg.GetPackageDirectory(_existingPackage.Id, _existingPackage.Version), Times.Once(),
                "Should  have called PackageFileGetter.GetPackageDirectory");
        }

        [Test]
        public void ShouldCallDeleteDirectoryIfEmpty() {
            const string packageDirectory = "path";
            _mockedPackageFileGetter.Setup(pfg => pfg.GetPackageDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(packageDirectory);

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedFileSystem.Verify(fs => fs.DeleteDirectoryIfEmpty(packageDirectory, true), Times.Once(),
                "DeleteDirectoryIfEmpty on IFileSystem should have been called.");
        }

        [Test]
        public void ShouldCallDeleteManyInScreenshotRepository() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedScreenshotRespository.Verify(sr => sr.DeleteMany(It.IsAny<Func<Screenshot, bool>>()), Times.Once(),
                "ScreenshotRepository's DeleteMany() should have been called.");
        }

        [Test]
        public void ShouldCallDeleteSingleInPackageRepository() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageRepository.Verify(pr => pr.DeleteSingle(It.IsAny<Func<Package, bool>>()), Times.Once(),
                "PackageRepository's Delete() should have been called.");
        }

        [Test]
        public void ShouldCallDeleteManyInPublishedScreenshotRepository() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPublishedScreenshotRepository.Verify(psr => psr.DeleteMany(It.IsAny<Func<PublishedScreenshot, bool>>()), Times.Once(),
                "PublishedScreenshotRepository's DeleteMany() should have been called.");
        }

        [Test]
        public void ShouldCallDeleteManyInPublishedPackageRepository() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPublishedPackageRepository.Verify(ppr => ppr.DeleteMany(It.IsAny<Func<PublishedPackage, bool>>()), Times.Once(),
                "PublishedPackageRepository's DeleteMany() should have been called.");
        }

        [Test]
        public void ShouldCallDeleteManyInDependencyRepository() {
            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedDependencyRepository.Verify(dr => dr.DeleteMany(It.IsAny<Func<Dependency, bool>>()), Times.Once(),
                "DependencyRepository's DeleteMany() should have been called.");
        }

        [Test]
        public void ShouldCallPackageRecommendedVersionManagerIfPackageIsRecommended()
        {
            _existingPackage.IsLatestVersion = true;

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageRecommendedVersionManager.Verify(prvm => prvm.SetLatestVersionAsRecommended(_existingPackage.Id, false), Times.Once());
            _mockedPublishedPackageRecommendedVersionManager.Verify(pprvm => pprvm.SetLatestVersionAsRecommended(_existingPackage.Id, true), Times.Once());
        }

        [Test]
        public void ShouldNotCallPackageRecommendedVersionManagerIfPackageIsNotRecommended()
        {
            _existingPackage.IsLatestVersion = false;

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageRecommendedVersionManager.Verify(prvm => prvm.SetLatestVersionAsRecommended(_existingPackage.Id, false), Times.Never());
            _mockedPublishedPackageRecommendedVersionManager.Verify(pprvm => pprvm.SetLatestVersionAsRecommended(_existingPackage.Id, true), Times.Never());
        }

        [Test]
        public void ShouldThrowWhenGivenIdAndVersionThatDoNotExist()
        {
            TestDelegate methodThatShouldThrow = () => _packageDeleter.DeletePackage("nonexisting-id", _existingPackage.Version);

            Assert.Throws<PackageDoesNotExistException>(methodThatShouldThrow, "Should have thrown correct exception when Package could not be found.");
        }

        [Test]
        public void ShouldUpdatePackageAggregatesWhenAnotherPackageWithSameIdExists()
        {
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(new[]
            {
                new Package {Id = _existingPackage.Id, Version = "2.0"},
                _existingPackage
            }.AsQueryable());

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageDataAggregateUpdater.Verify(pdau => pdau.RecalculateTotalDownloadCount(_existingPackage.Id), Times.Once());
            _mockedPackageDataAggregateUpdater.Verify(pdau => pdau.UpdateAggregateRatings(It.Is<IEnumerable<PackageVersionRatings>>(pvr => CorrectCollectionOfRatingsGiven(pvr))),
                Times.Once());
        }

        [Test]
        public void ShouldNotUpdatePackageAggregatesWhenDeletingLastVersionOfPackageId()
        {
            List<Package> packages = new[]
            {
                new Package {Id = "DifferentId", Version = "2.0"},
                _existingPackage
            }.ToList();

            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPackageRepository.Setup(pr => pr.DeleteSingle(It.IsAny<Func<Package, bool>>())).Callback(() => packages.Remove(_existingPackage));

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageDataAggregateUpdater.Verify(pdau => pdau.RecalculateTotalDownloadCount(_existingPackage.Id), Times.Never());
            _mockedPackageDataAggregateUpdater.Verify(pdau => pdau.UpdateAggregateRatings(It.Is<IEnumerable<PackageVersionRatings>>
                (pvr => CorrectCollectionOfRatingsGiven(pvr))), Times.Never());
        }

        [Test]
        public void ShouldDeletePackageDataAggregateWhenDeletingLastVersionOfPackageId()
        {
            var packages = new List<Package> { _existingPackage};
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPackageRepository.Setup(pr => pr.DeleteSingle(It.IsAny<Func<Package, bool>>())).Callback(() => packages.Remove(_existingPackage));
            _mockedPackageDataAggregateRepo.SetupGet(pdar => pdar.Collection).Returns(new[]
            {
                new PackageDataAggregate {PackageId = _existingPackage.Id}
            }.AsQueryable());

            _packageDeleter.DeletePackage(_existingPackage.Id, _existingPackage.Version);

            _mockedPackageDataAggregateRepo.Verify(pda => pda.DeleteSingle(It.IsAny<Func<PackageDataAggregate, bool>>()), Times.Once());
        }

        private bool CorrectCollectionOfRatingsGiven(IEnumerable<PackageVersionRatings> ratingsCollection)
        {
            if (ratingsCollection.Count() != 1)
            {
                return false;
            }
            PackageVersionRatings packageVersionRatings = ratingsCollection.Single();
            return packageVersionRatings.PackageId == _existingPackage.Id &&
                packageVersionRatings.PackageVersion == _existingPackage.Version &&
                packageVersionRatings.RatingAverage == 0 &&
                packageVersionRatings.RatingCount == 0;
        }
    }
}