using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Contants;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.UnfinishedPackageGetter
{
    [TestFixture]
    public class GetUnfinishedPackagesTester
    {
        private Infrastructure.Impl.UnfinishedPackageGetter _unfinishedPackageGetter;
        private Mock<IRepository<Package>> _mockedPackageRepository;
        private Mock<IRepository<PublishedPackage>> _mockedPublishedPackageRepository;
        private Mock<IPackageAuthenticator> _mockedPackageAuthenticator;

        [SetUp]
        public void SetUp()
        {
            _mockedPackageRepository = new Mock<IRepository<Package>>();
            _mockedPackageAuthenticator = new Mock<IPackageAuthenticator>();
            _mockedPublishedPackageRepository = new Mock<IRepository<PublishedPackage>>();
            _unfinishedPackageGetter = new Infrastructure.Impl.UnfinishedPackageGetter(_mockedPackageRepository.Object, _mockedPackageAuthenticator.Object,
                _mockedPublishedPackageRepository.Object);
        }

        private void ConfigureMockedRepositoryWithTestData()
        {
            var repoPackages = new List<Package>
            {
                new Package {Id = "Published1", Published = DateTime.UtcNow},
                new Package {Id = "Published2", Published = DateTime.UtcNow},
                new Package {Id = "Unpublished1", Published = Constants.UnpublishedDate},
                new Package {Id = "Unpublished2", Published = Constants.UnpublishedDate}
            };
            _mockedPackageRepository.Setup(pr => pr.Collection).Returns(repoPackages.AsQueryable());
        }

        [Test]
        public void ShouldReturnEmptyListWhenRequestedPackagesListIsEmpty()
        {
            ConfigureMockedRepositoryWithTestData();

            var requestedPackages = new List<string>();
            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), requestedPackages);

            CollectionAssert.IsEmpty(unfinishedPackages, "No packages should have been returned.");
        }

        [Test]
        public void ShouldReturnEmptyListWhenRequestIdsMatchNoExistingPackages()
        {
            ConfigureMockedRepositoryWithTestData();

            var requestedPackages = new List<string> { "UnknownId" };
            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), requestedPackages);

            CollectionAssert.IsEmpty(unfinishedPackages, "No packages should have been returned.");
        }

        [Test]
        public void ShouldNotReturnEmptyListWhenRequestIdsMatchExistingUnfinishedPackages()
        {
            ConfigureMockedRepositoryWithTestData();

            var requestedPackages = new List<string> { "Unpublished1" };
            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), requestedPackages);

            CollectionAssert.IsNotEmpty(unfinishedPackages, "Some packages should have been returned.");
        }

        [Test]
        public void ShouldReturnListWithOneItemWhenOneRequestedIdMatchesAnExistingUnfinishedPackage()
        {
            ConfigureMockedRepositoryWithTestData();

            var requestedPackages = new List<string> { "Unpublished1" };
            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), requestedPackages);

            Assert.AreEqual(1, unfinishedPackages.Count(), "One package should have been returned.");
        }

        [Test]
        public void ShouldReturnCorrectPackageWhenOneRequestedIdMatchesAnExistingUnfinishedPackage()
        {
            ConfigureMockedRepositoryWithTestData();

            var requestedPackages = new List<string> { "Unpublished1" };
            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), requestedPackages);

            var package = unfinishedPackages.First();
            Assert.AreEqual("Unpublished1", package.Id, "Incorrect package returned.");
        }

        [Test]
        public void ShouldCallEnsureKeyCanAccessPackagesUsingGivenPackageIdsAndKey()
        {
            string[] packageIds = new[] { "Foo", "Bar" };
            string key = Guid.NewGuid().ToString();

            _unfinishedPackageGetter.GetUnfinishedPackages(key, packageIds);

            _mockedPackageAuthenticator.Verify(pa => pa.EnsureKeyCanAccessPackages(packageIds, key), Times.Once(), "Should have authorized PackageIds.");
        }

        [Test]
        public void ShouldReturnEmptyCollectionWhenGivenEmptyListOfPackageIds()
        {
            IEnumerable<string> emptyCollection = new string[0];

            IEnumerable<Package> packages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), emptyCollection);

            CollectionAssert.IsEmpty(packages, "Empty collection should have been returned.");
        }

        [Test]
        public void ShouldReturnEmptyCollectionWhenGivenNullListOfPackageIds()
        {
            IEnumerable<string> nullCollection = null;

            IEnumerable<Package> packages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), nullCollection);

            CollectionAssert.IsEmpty(packages, "Empty collection should have been returned.");
        }

        [Test]
        public void ShouldReturnEmptyListWhenAllPackagesAreOnFeed()
        {
            const string packageId = "Foo";
            const string packageVersion1 = "1.5.35";
            const string packageVersion2 = "5.88.8";
            var packages = new[] {new Package {Id = packageId, Version = packageVersion1}, new Package {Id = packageId, Version = packageVersion2}};
            var publishedPackages = packages.Select(p => new PublishedPackage {Id = p.Id, Version = p.Version});
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages.AsQueryable());

            IEnumerable<Package> unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), new[] {packageId});

            CollectionAssert.IsEmpty(unfinishedPackages, "No Packages should have been returned.");
        }

        [Test]
        public void ShouldReturnPackageThatDoesNotExistOnFeed()
        {
            const string packageId = "Bar";
            const string packageVersion1 = "1.5.35";
            const string packageVersion2 = "5.88.8";
            var expectedPackage = new Package { Id = packageId, Version = packageVersion2 };
            var packages = new[] { new Package { Id = packageId, Version = packageVersion1 }, expectedPackage };
            var publishedPackages = new[] { new PublishedPackage { Id = packageId, Version = packageVersion1 } };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages.AsQueryable());

            IEnumerable<Package> unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), new[] { packageId });

            Assert.AreEqual(1, unfinishedPackages.Count(), "Only one Package should have been returned.");
            CollectionAssert.Contains(unfinishedPackages, expectedPackage, "Expected Package was not returned.");
        }

        [Test]
        public void ShouldReturnBothPackagesWhenNeitherExistOnFeed()
        {
            const string packageId = "Package";
            var packages = new[] { new Package { Id = packageId, Version = "1.5.35" }, new Package { Id = packageId, Version = "5.88.8" } };
            var publishedPackages = new PublishedPackage[0];
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages.AsQueryable());

            IEnumerable<Package> unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), new[] { packageId });

            Assert.AreEqual(2, unfinishedPackages.Count(), "Only one Package should have been returned.");
            CollectionAssert.AreEquivalent(packages, unfinishedPackages, "Expected Packages were not returned.");
        }

        [Test]
        public void ShouldReturnEmptyListWhenRequestIdsMatchNoExistingUnfinishedPackages()
        {
            var packages = new[] { new Package { Id = "Package", Version = "1.5.35" }, new Package { Id = "AnotherPackage", Version = "5.88.8" } };
            var publishedPackages = new PublishedPackage[0];
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages.AsQueryable());

            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), new[] { "Foo", "Bar"});

            CollectionAssert.IsEmpty(unfinishedPackages, "No packages should have been returned.");
        }

        [Test]
        public void ShouldReturnAllUnfinishedPackagesOfTheRequestedIds()
        {
            const string requestedId1 = "MySuperPackageId";
            const string requestedId2 = "MyOtherPackageId";
            const string requestedId3 = "AnotherPackage";
            var expectedPackage1 = new Package { Id = requestedId1, Version = "5.88.8" };
            var expectedPackage2 = new Package { Id = requestedId2, Version = "1.5.35" };
            var unexpectedPackage1 = new Package { Id = requestedId3, Version = "7.8.9" };
            var unexpectedPackage2 = new Package { Id = requestedId2, Version = "66.88.8" };

            var packages = new[]
            {
                expectedPackage2,
                new Package { Id = "SomePackage", Version = "3.4.5" },
                expectedPackage1,
                unexpectedPackage2,
                unexpectedPackage1,
            };
            var publishedPackages = new[]
            {
                new PublishedPackage { Id = unexpectedPackage2.Id, Version = unexpectedPackage2.Version },
                new PublishedPackage { Id = unexpectedPackage1.Id, Version = unexpectedPackage1.Version },
            };
            _mockedPackageRepository.SetupGet(pr => pr.Collection).Returns(packages.AsQueryable());
            _mockedPublishedPackageRepository.SetupGet(ppr => ppr.Collection).Returns(publishedPackages.AsQueryable());

            var unfinishedPackages = _unfinishedPackageGetter.GetUnfinishedPackages(Guid.NewGuid().ToString(), new[] { requestedId1, requestedId2, requestedId3 });

            CollectionAssert.Contains(unfinishedPackages, expectedPackage1, "Expected Package should have been returned.");
            CollectionAssert.Contains(unfinishedPackages, expectedPackage2, "Expected Package should have been returned.");
            CollectionAssert.DoesNotContain(unfinishedPackages, unexpectedPackage1, "Package should not have been returned since it was in the Feed.");
            CollectionAssert.DoesNotContain(unfinishedPackages, unexpectedPackage2, "Package should not have been returned since it was in the Feed.");
        }
    }
}