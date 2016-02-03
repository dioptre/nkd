using System;
using System.Collections.Generic;
using NuGet;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public interface IZipPackage
    {
        string Id { get; }
        Version Version { get; }
        string Title { get; }
        IEnumerable<string> Authors { get; }
        string Summary { get; }
        string Description { get; }
        string Language { get; }
        Uri LicenseUrl { get; }
        Uri ProjectUrl { get;}
        Uri IconUrl { get; }
        IEnumerable<PackageDependency> Dependencies { get; }
        string Tags { get; }
        bool RequireLicenseAcceptance { get; }
    }
}