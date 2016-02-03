using System;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class PackageReportAbuseUriCreator : IPackageReportAbuseUriCreator
    {
        private readonly IConfigSettings _configSettings;

        public PackageReportAbuseUriCreator(IConfigSettings configSettings)
        {
            _configSettings = configSettings;
        }

        public string CreateUri(PublishedPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            return _configSettings.ReportAbuseUriTemplate.Replace(PackageUriTemplateToken.PackageId, package.Id)
                .Replace(PackageUriTemplateToken.PackageVersion, package.Version);
        }
    }
}