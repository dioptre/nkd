namespace Gallery.UnitTests.PackageService
{
    public class RatePackageTester : PackageServiceTester
    {
        // TODO: Uncomment when Ratings are implemented ont he Gallery frontend.
        //[TestCase("Foo")]
        //[TestCase("  ")]
        //[TestCase("ABC123")]
        //public void ShouldThrowBadRequestExceptionWhenGivenNonNumericValueForRatingString(string invalidRating)
        //{
        //     TestDelegate methodThatShouldThrow = () => PackageService.RatePackage("id", "version", invalidRating);

        //     ExceptionAssert.ThrowsWebFaultException<string>(methodThatShouldThrow);
        //}

        //[Test]
        //public void ShouldInvokePackageRater()
        //{
        //    const string packageId = "id";
        //    const string packageVersion = "version";
        //    const int rating = 5;

        //    PackageService.RatePackage(packageId, packageVersion, rating.ToString());

        //    MockedPackageRater.Verify(pr => pr.RatePackage(packageId, packageVersion, rating), Times.Once(),
        //        "PackageRater was not invoked with the correct parameters.");
        //}
    }
}