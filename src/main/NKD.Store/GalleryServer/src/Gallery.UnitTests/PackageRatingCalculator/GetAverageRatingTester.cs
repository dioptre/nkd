using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageRatingCalculator
{
    [TestFixture]
    public class GetAverageRatingTester
    {
        private IPackageRatingCalculator _packageRatingCalculator;

        [SetUp]
        public void SetUp()
        {
            _packageRatingCalculator = new Core.Impl.PackageRatingCalculator();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void ShouldReturn0WhenGivenCountOf0AndAnySum(int ratingSum)
        {
            const int ratingCount = 0;
            const double expectedAverageRating = 0;

            double averageRating = _packageRatingCalculator.GetAverageRating(ratingSum, ratingCount);

            Assert.AreEqual(expectedAverageRating, averageRating, "AverageRating should have been 0.");
        }

        [TestCase(1, 5, .2)]
        [TestCase(0, 15, 0)]
        [TestCase(13, 25, .52)]
        public void ShouldReturnSumDividedByCount(int ratingSum, int ratingCount, double expectedAverageRating)
        {
            double averageRating = _packageRatingCalculator.GetAverageRating(ratingSum, ratingCount);

            Assert.AreEqual(expectedAverageRating, averageRating, "AverageRating calculated incorrectly.");
        }
    }
}