using System;
using System.Collections.Generic;
using NuGet;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public class NuPackZipPackage : IZipPackage
    {
        private readonly ZipPackage _zipPackage;

        public NuPackZipPackage(ZipPackage zipPackage)
        {
            _zipPackage = zipPackage;
        }

        public string Id { get { return _zipPackage.Id; } }
        public Version Version { get { return _zipPackage.Version; } }
        public string Title { get { return _zipPackage.Title; } }
        public IEnumerable<string> Authors { get { return _zipPackage.Authors; } }
        public string Summary { get { return _zipPackage.Summary; } }
        public string Description { get { return _zipPackage.Description; } }
        public string Language { get { return _zipPackage.Language; } }
        public Uri LicenseUrl { get { return _zipPackage.LicenseUrl; } }
        public Uri ProjectUrl { get { return _zipPackage.ProjectUrl; } }
        public Uri IconUrl { get { return _zipPackage.IconUrl; } }
        public IEnumerable<PackageDependency> Dependencies { get { return _zipPackage.Dependencies; } }
        public string Tags { get { return _zipPackage.Tags; } }
        public bool RequireLicenseAcceptance { get { return _zipPackage.RequireLicenseAcceptance; } }
    }
}