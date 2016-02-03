using System;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageSlugCreator : IPackageSlugCreator
    {
        public string CreateSlug(string packageId, string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentNullException("packageId");
            }
            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new ArgumentNullException("packageVersion");
            }
            return string.Format("{0}-{1}", packageId, packageVersion).Replace('.', '-').Replace(' ', '-');
        }
    }
}