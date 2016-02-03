using System;
using System.IO;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageCreator : IPackageCreator
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGuid _guid;
        private readonly IPackageFactory _packageFactory;
        private readonly IConfigSettings _configSettings;
        private readonly IRepository<Package> _packageRepository;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IHashGetter _hashGetter;
        private readonly IPackageIdValidator _packageIdValidator;
        private readonly IPackageUriValidator _packageUriValidator;
        private readonly ILatestVersionChecker _latestVersionChecker;
        private readonly ILatestVersionUpdater<Package> _latestVersionUpdater;

        public PackageCreator(IFileSystem fileSystem, IGuid guid, IPackageFactory packageFactory, IConfigSettings configSettings,
            IRepository<Package> packageRepository, IPackageAuthenticator packageAuthenticator, IHashGetter hashGetter,
            IPackageIdValidator packageIdValidator, IPackageUriValidator packageUriValidator,
            ILatestVersionChecker latestVersionChecker, ILatestVersionUpdater<Package> latestVersionUpdater)
        {
            _fileSystem = fileSystem;
            _latestVersionUpdater = latestVersionUpdater;
            _latestVersionChecker = latestVersionChecker;
            _packageIdValidator = packageIdValidator;
            _guid = guid;
            _packageFactory = packageFactory;
            _configSettings = configSettings;
            _packageRepository = packageRepository;
            _packageAuthenticator = packageAuthenticator;
            _hashGetter = hashGetter;
            _packageUriValidator = packageUriValidator;
        }

        public Package CreatePackage(string key, Stream packageFileStream, string fileExtension, bool isInPlaceUpdate, string externalPackageUri)
        {
            string tempPath = GenerateNewTempPath();
            Package package;

            try
            {
                package = ExtractPackage(packageFileStream, tempPath);
                if (!_packageIdValidator.IsValidPackageId(package.Id))
                {
                    throw new InvalidPackageIdException(package.Id);
                }
                _packageUriValidator.ValidatePackageUris(package);
                _packageAuthenticator.EnsureKeyCanAccessPackage(key, package.Id, package.Version);
                GenerateHash(tempPath, package);
                if (string.IsNullOrWhiteSpace(package.Summary))
                {
                    package.Summary = package.Description;
                }

                if (!isInPlaceUpdate)
                {
                    CreateNewPackage(tempPath, package, externalPackageUri, fileExtension);
                }
                else
                {
                    UpdatePackageInPlace(tempPath, package, externalPackageUri, fileExtension);
                }
            }
            finally
            {
                _fileSystem.DeleteFileIfItExists(tempPath);
            }
            return package;
        }

        private void CreateNewPackage(string tempPath, Package package, string externalPackageUri, string fileExtension)
        {
            VerifyPackageDoesNotAlreadyExist(package.Id, package.Version);
            MovePackageFile(externalPackageUri, tempPath, package, fileExtension);
            package.IsLatestVersion = _latestVersionChecker.IsLatestVersion(package.Id, package.Version);
            _latestVersionUpdater.SetLatestVersionFlagsOfOtherVersionablesWithSameId(package);
            _packageRepository.Create(package);
        }

        private void UpdatePackageInPlace(string tempPath, Package package, string externalPackageUri, string fileExtension)
        {
            var existingPackage = _packageRepository.Collection.FirstOrDefault(p => p.Id == package.Id && p.Version == package.Version);
            if (existingPackage == null)
            {
                string message = string.Format("A Package with ID '{0}' and Version '{1}' does not exist on the server. In place update cannot take place.",
                    package.Id, package.Version);
                throw new Exception(message);
            }
            package.Created = existingPackage.Created;
            package.IsLatestVersion = existingPackage.IsLatestVersion;
            MovePackageFile(externalPackageUri, tempPath, package, fileExtension);
            _packageRepository.Update(package);
        }

        private void MovePackageFile(string externalPackageUri, string tempPath, Package package, string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(externalPackageUri))
            {
                string fullPackagePath = Path.Combine(_configSettings.PhysicalSitePath, _configSettings.RelativePackageDirectory);
                string packagePath = string.Format(@"{0}\{1}\{1}-{2}.{3}", fullPackagePath, package.Id, package.Version, fileExtension);
                _fileSystem.Move(tempPath, packagePath, true);
            }
            else
            {
                package.ExternalPackageUrl = externalPackageUri;
            }
        }

        private void VerifyPackageDoesNotAlreadyExist(string packageId, string packageVersion)
        {
            if (_packageRepository.Collection.Any(p => p.Id == packageId && p.Version == packageVersion))
            {
                string message = string.Format("A Package with ID '{0}' and Version '{1}' already exists on the server.", packageId, packageVersion);
                throw new Exception(message);
            }
        }

        private void GenerateHash(string tempPath, Package package)
        {
            ComputedHash hash;
            using (var tempStream = _fileSystem.OpenRead(tempPath))
            {
                hash = _hashGetter.GetHashFromFile(tempStream);
            }
            package.PackageHash = hash.ComputedHashCode;
            package.PackageHashAlgorithm = hash.HashingAlgorithmUsed.ToString();
        }

        private Package ExtractPackage(Stream packageFileStream, string tempPath)
        {
            _fileSystem.Save(packageFileStream, tempPath);
            return _packageFactory.CreateNewFromFile(tempPath);
        }

        private string GenerateNewTempPath()
        {
            string tempFileName = _guid.NewGuid().ToString();
            string fullTempPath = Path.Combine(_configSettings.PhysicalSitePath, _configSettings.RelativeTemporaryDirectory);
            return string.Format(@"{0}\{1}", fullTempPath, tempFileName);
        }
    }
}