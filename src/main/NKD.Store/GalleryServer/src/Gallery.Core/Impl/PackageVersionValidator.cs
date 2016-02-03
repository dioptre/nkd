using System;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageVersionValidator : IPackageVersionValidator
    {
        public bool IsValidPackageVersion(string packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                return false;
            }
            Version outParam;
            return Version.TryParse(packageVersion, out outParam);
        }
    }
}