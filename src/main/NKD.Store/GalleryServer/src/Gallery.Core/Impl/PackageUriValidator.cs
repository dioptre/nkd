using System;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageUriValidator : IPackageUriValidator
    {
        private readonly IGalleryUriValidator _galleryUriValidator;

        public PackageUriValidator(IGalleryUriValidator galleryUriValidator)
        {
            _galleryUriValidator = galleryUriValidator;
        }

        public void ValidatePackageUris(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            ValidatePackageUri(package.ExternalPackageUrl, UriKind.Absolute);
            ValidatePackageUri(package.LicenseUrl, UriKind.Absolute);
            ValidatePackageUri(package.ProjectUrl, UriKind.Absolute);
            ValidatePackageUri(package.IconUrl, UriKind.RelativeOrAbsolute);
        }

        private void ValidatePackageUri(string packageUri, UriKind allowedUriKind)
        {
            if (!_galleryUriValidator.IsValidUri(packageUri, allowedUriKind))
            {
                throw new UriFormatException(packageUri);
            }
        }
    }
}