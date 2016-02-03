namespace Gallery.Core.Domain
{
    public class PackageVersionRatings
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public int RatingCount { get; set; }
        public double RatingAverage { get; set; }
    }
}