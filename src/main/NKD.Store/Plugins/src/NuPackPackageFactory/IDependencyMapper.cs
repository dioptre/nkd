using System.Collections.Generic;
using Gallery.Core.Domain;
using NuGet;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public interface IDependencyMapper
    {
        Dependency Map(PackageDependency packageDependency, string packageId, string packageVersion);
        IEnumerable<Dependency> Map(IEnumerable<PackageDependency> packageDependencies, string packageId, string packageVersion);
    }
}