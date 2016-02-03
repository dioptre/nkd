namespace Gallery.Core.Interfaces
{
    public interface IPackageRatingCalculator
    {
        double GetAverageRating(int ratingSum, int ratingsCount);
    }
}