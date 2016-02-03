using System.ComponentModel.DataAnnotations;

namespace Gallery.Core.Domain
{
    public class Dependency
    {
        public int Id { get; set; }
        [Required]
        public string PackageId { get; set; }
        [Required]
        public string PackageVersion { get; set; }
        public string Name { get; set; }
        public string VersionSpec { get; set; }
    }
}