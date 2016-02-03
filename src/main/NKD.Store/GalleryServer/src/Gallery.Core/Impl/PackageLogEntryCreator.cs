using System;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageLogEntryCreator : IPackageLogEntryCreator
    {
        private readonly IDateTime _dateTime;
        private readonly IRepository<PackageLogEntry> _packageLogEntryRepository;

        public PackageLogEntryCreator(IDateTime dateTime, IRepository<PackageLogEntry> packageLogEntryRepository)
        {
            _dateTime = dateTime;
            _packageLogEntryRepository = packageLogEntryRepository;
        }

        public void Create(string packageId, string packageVersion, PackageLogAction action)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentNullException("packageId");
            }
            if (string.IsNullOrWhiteSpace(packageVersion))
            {
                throw new ArgumentNullException("packageVersion");
            }
            var packageLogEntry = new PackageLogEntry
            {
                PackageId = packageId,
                PackageVersion = packageVersion,
                DateLogged = _dateTime.UtcNow,
                Action = action
            };
            _packageLogEntryRepository.Create(packageLogEntry);
        }
    }
}