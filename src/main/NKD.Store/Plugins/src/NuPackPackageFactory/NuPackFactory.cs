using System;
using System.IO;
using Gallery.Core.Domain;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using NuGet;
using IFileSystem = Gallery.Core.Interfaces.IFileSystem;
using IPackageFactory = Gallery.Core.Interfaces.IPackageFactory;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public class NuPackPackageFactory : IPackageFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly IPackageMapper<IZipPackage> _packageMapper;
        private readonly IDateTime _dateTime;

        public NuPackPackageFactory()
            : this (new WindowsFileSystem(), new ZipPackageMapper(), new SystemDateTime())
        { }

        public NuPackPackageFactory(IFileSystem fileSystem, IPackageMapper<IZipPackage> packageMapper, IDateTime dateTime)
        {
            _fileSystem = fileSystem;
            _packageMapper = packageMapper;
            _dateTime = dateTime;
        }

        public Package CreateNewFromFile(string pathToPackageFile)
        {
            long packageFileSize;
            using (FileStream packageFileStream = _fileSystem.OpenRead(pathToPackageFile))
            {
                packageFileSize = packageFileStream.Length;
            }
            IZipPackage zipPackage = new NuPackZipPackage(new ZipPackage(pathToPackageFile));
            DateTime currentTime = _dateTime.UtcNow;
            Package package = _packageMapper.Map(zipPackage);
            package.PackageSize = packageFileSize;
            package.Created = currentTime;
            package.LastUpdated = currentTime;
            package.Tags = zipPackage.Tags;
            return package;
        }
    }
}