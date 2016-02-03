using System;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageLogEntryCreator
{
    [TestFixture]
    public class CreateTester
    {
        private IPackageLogEntryCreator _packageLogEntryCreator;

        private Mock<IDateTime> _mockedDateTime;
        private Mock<IRepository<PackageLogEntry>> _mockedPackageLogEntryRepository;

        [SetUp]
        public void SetUp()
        {
            _mockedDateTime = new Mock<IDateTime>();
            _mockedPackageLogEntryRepository = new Mock<IRepository<PackageLogEntry>>();

            _packageLogEntryCreator = new Core.Impl.PackageLogEntryCreator(_mockedDateTime.Object, _mockedPackageLogEntryRepository.Object);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ShouldThrowWhenGivenNullOrEmptyOrWhitespacePackageId(string packageId)
        {
            TestDelegate methodThatShouldThrow = () => _packageLogEntryCreator.Create(packageId, "version", PackageLogAction.Create);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhitespacePackageVersion(string packageVersion)
        {
            TestDelegate methodThatShouldThrow = () => _packageLogEntryCreator.Create("id", packageVersion, PackageLogAction.Create);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldSetPackageIdToGivenId()
        {
            const string expectedPackageId = "id";

            _packageLogEntryCreator.Create(expectedPackageId, "version", PackageLogAction.Create);

            _mockedPackageLogEntryRepository.Verify(pler => pler.Create(It.Is<PackageLogEntry>(ple => ple.PackageId == expectedPackageId)), Times.Once(),
                "PackageId was not set to the given Id.");
        }

        [Test]
        public void ShouldSetPackageVersionToGivenVersion()
        {
            const string expectedVersion = "version";

            _packageLogEntryCreator.Create("id", expectedVersion, PackageLogAction.Create);

            _mockedPackageLogEntryRepository.Verify(pler => pler.Create(It.Is<PackageLogEntry>(ple => ple.PackageVersion == expectedVersion)), Times.Once(),
                "PackageVersion was not set to the given Version.");
        }

        [Test]
        public void ShouldSetPackageLogActionToGivenAction()
        {
            const PackageLogAction expectedAction = PackageLogAction.Update;

            _packageLogEntryCreator.Create("id", "version", expectedAction);

            _mockedPackageLogEntryRepository.Verify(pler => pler.Create(It.Is<PackageLogEntry>(ple => ple.Action == expectedAction)), Times.Once(),
                "Action was not set to the given PackageLogAction.");
        }

        [Test]
        public void ShouldSetDateDeletedToUtcNow()
        {
            DateTime expectedDate = DateTime.Now;
            _mockedDateTime.SetupGet(dt => dt.UtcNow).Returns(expectedDate);

            _packageLogEntryCreator.Create("id", "version", PackageLogAction.Create);

            _mockedPackageLogEntryRepository.Verify(pler => pler.Create(It.Is<PackageLogEntry>(ple => ple.DateLogged == expectedDate)), Times.Once(),
                "DateLogged was not set to UtcNow.");
        }

        [Test]
        public void ShouldCreateEntryInRepository()
        {
            _packageLogEntryCreator.Create("id", "version", PackageLogAction.Create);

            _mockedPackageLogEntryRepository.Verify(pler => pler.Create(It.IsAny<PackageLogEntry>()), Times.Once(),
                "Created PackageLogEntry should be passed to Repository.");
        }
    }
}