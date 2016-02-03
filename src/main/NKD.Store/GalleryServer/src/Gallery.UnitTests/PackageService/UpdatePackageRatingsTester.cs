using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.UnitTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageService
{
    public class UpdatePackageRatingsTester : PackageServiceTester
    {
        [Test]
        public void ShouldCallPackageRatingUpdater()
        {
            var expectedRatingAggregates = new List<PackageVersionRatings>();

            PackageService.UpdatePackageRatings(expectedRatingAggregates, "");

            MockedPackageRatingUpdater.Verify(pru => pru.UpdatePackageRatings(expectedRatingAggregates), Times.Once());
        }

        [Test]
        public void ShouldThrowWebFaultWhenUpdaterThrows()
        {
            MockedPackageRatingUpdater.Setup(pru => pru.UpdatePackageRatings(It.IsAny<IEnumerable<PackageVersionRatings>>())).Throws(new Exception());

            TestDelegate methodThatShouldThrow = () => PackageService.UpdatePackageRatings(null, "");

            ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow);
        }
    }
}