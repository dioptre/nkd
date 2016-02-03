using System.ComponentModel.DataAnnotations;

namespace Gallery.Core.Domain
{
    public class Screenshot
    {
        public int Id { get; set; }
        [Required]
        public string PackageId { get; set; }
        [Required]
        public string PackageVersion { get; set; }
        public string ScreenshotUri { get; set; }
        public string Caption { get; set; }
    }
}