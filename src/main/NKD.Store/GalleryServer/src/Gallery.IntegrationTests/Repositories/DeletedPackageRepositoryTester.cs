using System;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.IntegrationTests.Helpers;

namespace Gallery.IntegrationTests.Repositories
{
    public class DeletedPackageRepositoryTester : RepositoryTesterBase<PackageLogEntry>
    {
        private readonly DateTime _dateDeleted = DateTime.UtcNow;

        protected override PackageLogEntry GetObjectToCreate()
        {
            Package existingPackage = PackageTestHelpers.GetExistingPackage();
            return new PackageLogEntry
            {
                PackageId = existingPackage.Id,
                PackageVersion = existingPackage.Version,
                DateLogged = _dateDeleted,
                Action = PackageLogAction.Delete,
            };
        }

        protected override Func<PackageLogEntry, bool> GetDeletionPredicate()
        {
            return dp => dp.DateLogged == _dateDeleted;
        }
    }
}