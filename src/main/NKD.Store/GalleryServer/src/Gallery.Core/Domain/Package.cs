using System;
using System.Collections.Generic;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Domain
{
    public class Package : IVersionable
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string PackageType { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public int DownloadCount { get; set; }
        public string Copyright { get; set; }
        public string Language { get; set; }
        public double RatingAverage { get; set; }
        public int RatingsCount { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public string PackageHash { get; set; }
        public long PackageSize { get; set; }
        public decimal Price { get; set; }
        public string Tags { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public bool IsLatestVersion { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime? Published { get; set; }

        public string ExternalPackageUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string IconUrl { get; set; }

        public IList<Screenshot> Screenshots { get; set; }
        public string Categories { get; set; }
        public IList<Dependency> Dependencies { get; set; }
    }
}