using System.ComponentModel.DataAnnotations;
using System.Data.Services.Common;

namespace Gallery.Infrastructure.FeedModels
{
    [DataServiceKey("Id")]
    public class PublishedScreenshot
    {
        public int Id { get; set; }
        [Required]
        public string PublishedPackageId { get; set; }
        [Required]
        public string PublishedPackageVersion { get; set; }
        public string ScreenshotUri { get; set; }
        public string Caption { get; set; }
    }
}