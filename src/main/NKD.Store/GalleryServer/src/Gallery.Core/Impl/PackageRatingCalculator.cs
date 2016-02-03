using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageRatingCalculator : IPackageRatingCalculator
    {
        private const int MINIMUM_RATING_COUNT = 0;

        public double GetAverageRating(int ratingSum, int ratingsCount)
        {
            return ratingsCount > MINIMUM_RATING_COUNT ? (double)ratingSum / ratingsCount : 0;
        }
    }
}