using System;
using System.IO;
using System.Linq;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Core.Extensions;

namespace Gallery.Core.Impl
{
    public class PackageFileGetter : IPackageFileGetter
    {
        private readonly IConfigSettings _configSettings;
        private readonly IFileSystem _fileSystem;

        public PackageFileGetter(IConfigSettings configSettings, IFileSystem fileSystem)
        {
            _configSettings = configSettings;
            _fileSystem = fileSystem;
        }

        public Stream GetPackageStream(string packageId, string packageVersion)
        {
            return _fileSystem.OpenRead(GetPackagePath(packageId, packageVersion));
        }

        public string GetPackageDirectory(string packageId, string packageVersion)
        {
            string fullPackagePath = Path.Combine(_configSettings.PhysicalSitePath, _configSettings.RelativePackageDirectory);
            return Path.Combine(fullPackagePath, packageId);
        }

        public string GetPackagePath(string packageId, string packageVersion)
        {
            try
            {
                string fileExtension = "nupkg"; //TODO: Make this a config setting
                string packageDirectory = GetPackageDirectory(packageId, packageVersion);
                string packageFileName = string.Format("{0}-{1}.{2}", packageId, packageVersion, fileExtension);

                return Path.Combine(packageDirectory, packageFileName);
            }
            catch (Exception exception)
            {
                throw new PackageFileDoesNotExistException(packageId, packageVersion, exception);
            }
        }
    }
}