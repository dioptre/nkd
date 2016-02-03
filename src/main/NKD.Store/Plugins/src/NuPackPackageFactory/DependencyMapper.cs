using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using NuGet;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public class DependencyMapper : IDependencyMapper
    {
        public Dependency Map(PackageDependency packageDependency, string packageId, string packageVersion)
        {
            if (packageDependency == null)
            {
                throw new ArgumentNullException("packageDependency");
            }
            string versionSpec = packageDependency.VersionSpec != null ? packageDependency.VersionSpec.ToString() : string.Empty;
            return new Dependency
            {
                Name = packageDependency.Id,
                PackageId = !string.IsNullOrWhiteSpace(packageId) ? packageId : string.Empty,
                PackageVersion = !string.IsNullOrWhiteSpace(packageVersion) ? packageVersion : string.Empty,
                VersionSpec = versionSpec
            };
        }

        public IEnumerable<Dependency> Map(IEnumerable<PackageDependency> packageDependencies, string packageId, string packageVersion)
        {
            return packageDependencies.Select(d => Map(d, packageId, packageVersion));
        }
    }
}