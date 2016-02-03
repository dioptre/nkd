using Gallery.Core.Domain;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly ILatestVersionUpdater<Package> _latestVersionUpdater;
        private readonly IPackageUriValidator _packageUriValidator;

        public PackageUpdater(IRepository<Package> packageRepository, IPackageUriValidator packageUriValidator,
            ILatestVersionUpdater<Package> latestVersionUpdater)
        {
            _packageRepository = packageRepository;
            _latestVersionUpdater = latestVersionUpdater;
            _packageUriValidator = packageUriValidator;
        }

        public void UpdateExistingPackage(Package packageToUpdate)
        {
            _packageUriValidator.ValidatePackageUris(packageToUpdate);
            _latestVersionUpdater.SetLatestVersionFlagsOfOtherVersionablesWithSameId(packageToUpdate);
            _packageRepository.Update(packageToUpdate);
        }
    }
}