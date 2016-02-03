using System;
using Gallery.Core.Domain;
using Gallery.Infrastructure.FeedModels;
using NUnit.Framework;
using System.Linq;

namespace Gallery.UnitTests.AutoMapper
{
    public class PackageToPublishedPackageTester : AutoMapperTester
    {
        [Test]
        public void RatingOnPublishedPackageShouldBeMappedTo0WhenAggregateForPackageIdDoesNotExist()
        {
            const double expectedRating = 0;
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new PackageDataAggregate[0].AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package());

            Assert.AreEqual(expectedRating, publishedPackage.Rating, "Rating should have been mapped to 0.");
        }

        [Test]
        public void RatingOnPublishedPackageShouldBeMappedToAggregateRatingWhenItExists()
        {
            PackageDataAggregate existingAggregate = new PackageDataAggregate { PackageId = Guid.NewGuid().ToString(), Rating = 3.58 };
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { existingAggregate }.AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package {Id = existingAggregate.PackageId});

            Assert.AreEqual(existingAggregate.Rating, publishedPackage.Rating, "Rating should have been mapped to aggregate rating.");
        }

        [Test]
        public void RatingsCountOnPublishedPackageShouldBeMappedTo0WhenAggregateForPackageIdDoesNotExist()
        {
            const int expectedRatingsCount = 0;
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new PackageDataAggregate[0].AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package());

            Assert.AreEqual(expectedRatingsCount, publishedPackage.RatingsCount, "RatingsCount should have been mapped to 0.");
        }

        [Test]
        public void RatingsCountOnPublishedPackageShouldBeMappedToAggregateRatingsCountWhenItExists()
        {
            PackageDataAggregate existingAggregate = new PackageDataAggregate { PackageId = Guid.NewGuid().ToString(), RatingsCount = 92 };
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { existingAggregate }.AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package { Id = existingAggregate.PackageId });

            Assert.AreEqual(existingAggregate.RatingsCount, publishedPackage.RatingsCount, "RatingsCount should have been mapped to aggregate rating.");
        }

        [Test]
        public void PackageRatingsCountShouldNotBeDirectlyMappedToRatingsCountInPublishedPackage()
        {
            Package packageToMapFrom = new Package { RatingsCount = 37 };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.AreNotEqual(packageToMapFrom.RatingsCount, publishedPackage.RatingsCount,
                "RatingsCount on PublishedPackage should not come from Package.RatingsCount.");
        }

        [Test]
        public void DownloadCountOnPublishedPackageShouldBeMappedTo0WhenAggregateForPackageIdDoesNotExist()
        {
            const int expectedDownloadCount = 0;
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new PackageDataAggregate[0].AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package());

            Assert.AreEqual(expectedDownloadCount, publishedPackage.DownloadCount, "DownloadCount should have been mapped to 0.");
        }

        [Test]
        public void DownloadCountOnPublishedPackageShouldBeMappedToAggregateDownloadCountWhenItExists()
        {
            PackageDataAggregate existingAggregate = new PackageDataAggregate { PackageId = Guid.NewGuid().ToString(), DownloadCount = 45 };
            MockedPackageDataAggregateRepository.SetupGet(pdar => pdar.Collection).Returns(new[] { existingAggregate }.AsQueryable());

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(new Package { Id = existingAggregate.PackageId });

            Assert.AreEqual(existingAggregate.DownloadCount, publishedPackage.DownloadCount, "DownloadCount should have been mapped to aggregate rating.");
        }

        [Test]
        public void PackageDownloadCountShouldNotBeDirectlyMappedToDownloadCountInPublishedPackage()
        {
            Package packageToMapFrom = new Package {DownloadCount = 45};

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.AreNotEqual(packageToMapFrom.DownloadCount, publishedPackage.DownloadCount,
                "DownloadCount on PublishedPackage should not come from Package.DownloadCount.");
        }

        [Test]
        public void CategoriesShouldBeDirectlyMapped()
        {
            Package packageToMapFrom = new Package {Categories = "Foo, Bar"};

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.AreEqual(packageToMapFrom.Categories, publishedPackage.Categories, "Categories were not mapped from Package to PublishedPackage correctly.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void TitleShouldBeNullWhenEmptyOrWhitespaceStringGiven(string title)
        {
            Package packageToMapFrom = new Package { Title = title };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Title, "Title should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void AuthorsShouldBeNullWhenEmptyOrWhitespaceStringGiven(string authors)
        {
            Package packageToMapFrom = new Package { Authors = authors };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Authors, "Authors should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void PackageTypeShouldBeNullWhenEmptyOrWhitespaceStringGiven(string packageType)
        {
            Package packageToMapFrom = new Package { PackageType = packageType };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.PackageType, "PackageType should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void DescriptionShouldBeNullWhenEmptyOrWhitespaceStringGiven(string description)
        {
            Package packageToMapFrom = new Package { Description = description };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Description, "Description should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void SummaryShouldBeNullWhenEmptyOrWhitespaceStringGiven(string summary)
        {
            Package packageToMapFrom = new Package { Summary = summary };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Summary, "Summary should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void CopyrightShouldBeNullWhenEmptyOrWhitespaceStringGiven(string copyright)
        {
            Package packageToMapFrom = new Package { Copyright = copyright };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Copyright, "Copyright should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void PackageHashAlgorithmShouldBeNullWhenEmptyOrWhitespaceStringGiven(string packageHashAlgorithm)
        {
            Package packageToMapFrom = new Package { PackageHashAlgorithm = packageHashAlgorithm };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.PackageHashAlgorithm, "PackageHashAlgorithm should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void PackageHashShouldBeNullWhenEmptyOrWhitespaceStringGiven(string packageHash)
        {
            Package packageToMapFrom = new Package { PackageHash = packageHash };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.PackageHash, "PackageHash should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void ExternalPackageUrlShouldBeNullWhenEmptyOrWhitespaceStringGiven(string externalPackageUrl)
        {
            Package packageToMapFrom = new Package { ExternalPackageUrl = externalPackageUrl };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.ExternalPackageUrl, "ExternalPackageUrl should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void ProjectUrlShouldBeNullWhenEmptyOrWhitespaceStringGiven(string projectUrl)
        {
            Package packageToMapFrom = new Package { ProjectUrl = projectUrl };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.ProjectUrl, "ProjectUrl should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void LicenseUrlShouldBeNullWhenEmptyOrWhitespaceStringGiven(string licenseUrl)
        {
            Package packageToMapFrom = new Package { LicenseUrl = licenseUrl };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.LicenseUrl, "LicenseUrl should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void IconUrlShouldBeNullWhenEmptyOrWhitespaceStringGiven(string iconUrl)
        {
            Package packageToMapFrom = new Package { IconUrl = iconUrl };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.IconUrl, "IconUrl should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void CategoriesShouldBeNullWhenEmptyOrWhitespaceStringGiven(string categories)
        {
            Package packageToMapFrom = new Package { Categories = categories };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Categories, "Categories should have been null.");
        }

        [TestCase("")]
        [TestCase("  ")]
        public void TagsShouldBeNullWhenEmptyOrWhitespaceStringGiven(string tags)
        {
            Package packageToMapFrom = new Package { Tags = tags };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.IsNull(publishedPackage.Tags, "Tags should have been null.");
        }

        [Test]
        public void RatingAverageShouldBeMappedToVersionRating()
        {
            Package packageToMapFrom = new Package {RatingAverage = 4.5};

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.AreEqual(packageToMapFrom.RatingAverage, publishedPackage.VersionRating, "VersionRating should come from RatingAverage.");
        }

        [Test]
        public void RatingsCountShouldBeMappedToVersionRatingsCount()
        {
            Package packageToMapFrom = new Package { RatingsCount = 66 };

            PublishedPackage publishedPackage = Mapper.Map<Package, PublishedPackage>(packageToMapFrom);

            Assert.AreEqual(packageToMapFrom.RatingsCount, publishedPackage.VersionRatingsCount, "VersionRatingCount should come from RatingsCount.");
        }
    }
}