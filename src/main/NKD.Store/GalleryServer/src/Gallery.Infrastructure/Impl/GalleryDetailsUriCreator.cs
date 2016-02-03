using System;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class GalleryDetailsUriCreator : IGalleryDetailsUriCreator
    {
        private readonly IConfigSettings _configSettings;

        public GalleryDetailsUriCreator(IConfigSettings configSettings)
        {
            _configSettings = configSettings;
        }

        public string CreateUri(PublishedPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            string packageTypeSlug = GallerySlugifyPackageType(package.PackageType);

            return _configSettings.GalleryDetailsUriTemplate.Replace(PackageUriTemplateToken.PackageTypeSlug, packageTypeSlug)
                .Replace(PackageUriTemplateToken.PackageId, package.Id)
                .Replace(PackageUriTemplateToken.PackageVersion, package.Version);
        }

        private string GallerySlugifyPackageType(string packageType)
        {
            var packageTypeSettingKey = string.Format("Gallery{0}Slug", packageType);
            var packageTypeSlug = _configSettings.TryGetString(packageTypeSettingKey);
            return string.IsNullOrWhiteSpace(packageTypeSlug) ? packageType : packageTypeSlug;
        }
    }
}