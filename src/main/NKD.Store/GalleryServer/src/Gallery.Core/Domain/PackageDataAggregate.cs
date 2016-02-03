using System.ComponentModel.DataAnnotations;

namespace Gallery.Core.Domain
{
    public class PackageDataAggregate
    {
        public int Id { get; set; }

        [Required]
        public string PackageId { get; set; }

        public double Rating { get; set; }

        public int RatingsCount { get; set; }

        public int DownloadCount { get; set; }
    }
}