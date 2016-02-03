using System;
using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class LatestVersionChecker : ILatestVersionChecker
    {
        private readonly IRepository<Package> _packageRepository;

        public LatestVersionChecker(IRepository<Package> packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public bool IsLatestVersion(string packageId, string packageVersion)
        {
            IQueryable<string> existingVersionStrings = _packageRepository.Collection.Where(p => p.Id == packageId).Select(p => p.Version);
            if (existingVersionStrings.Any())
            {
                Version existingGreatestVersion = existingVersionStrings.ToList().Select(v => new Version(v)).OrderByDescending(v => v).First();
                Version versionOfPackageToCreate = new Version(packageVersion);
                if (existingGreatestVersion > versionOfPackageToCreate)
                {
                    return false;
                }
            }
            return true;
        }
    }
}