using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Common;
using Gallery.Core;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.FeedModels
{
    [HasStream]
    [DataServiceKey("Id", "Version")]
    [EntityPropertyMapping("Title", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, true)]
    [EntityPropertyMapping("Authors", SyndicationItemProperty.AuthorName, SyndicationTextContentKind.Plaintext, true)]
    [EntityPropertyMapping("LastUpdated", SyndicationItemProperty.Updated, SyndicationTextContentKind.Plaintext, true)]
    [EntityPropertyMapping("Summary", SyndicationItemProperty.Summary, SyndicationTextContentKind.Plaintext, true)]
    public class PublishedPackage : IVersionable
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string PackageType { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public string PackageHash { get; set; }
        public long PackageSize { get; set; }
        public decimal Price { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public bool IsLatestVersion { get; set; }

        public double VersionRating { get; set; }
        public int VersionRatingsCount { get; set; }
        public int VersionDownloadCount { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime? Published { get; set; }

        public string ExternalPackageUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string IconUrl { get; set; }

        public double Rating { get; set; }
        public int RatingsCount { get; set; }
        public int DownloadCount { get; set; }

        public IList<PublishedScreenshot> Screenshots { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }
        public string Dependencies { get; set; }

        [NotMapped]
        public string ReportAbuseUrl { get { return _reportAbuseUriCreator.Value.CreateUri(this); } }
        private readonly Lazy<IPackageReportAbuseUriCreator> _reportAbuseUriCreator;

        [NotMapped]
        public string GalleryDetailsUrl { get { return _galleryDetailsUriCreator.Value.CreateUri(this); } }
        private readonly Lazy<IGalleryDetailsUriCreator> _galleryDetailsUriCreator;

        public PublishedPackage()
        {
            _reportAbuseUriCreator = new Lazy<IPackageReportAbuseUriCreator>(() => IoC.Resolver.Resolve<IPackageReportAbuseUriCreator>());
            _galleryDetailsUriCreator = new Lazy<IGalleryDetailsUriCreator>(() => IoC.Resolver.Resolve<IGalleryDetailsUriCreator>());
        }
    }
}