using System;
using Gallery.Core.Domain;
using Gallery.IntegrationTests.Helpers;

namespace Gallery.IntegrationTests.Repositories
{
    public class ScreenshotRepositoryTester : RepositoryTesterBase<Screenshot>
    {
        private readonly string _caption = string.Format("Integration Test - {0}", Guid.NewGuid());

        protected override Screenshot GetObjectToCreate()
        {
            Package existingPackage = PackageTestHelpers.GetExistingPackage();
            return new Screenshot
            {
                Caption = _caption,
                PackageId = existingPackage.Id,
                PackageVersion = existingPackage.Version
            };
        }

        protected override Func<Screenshot, bool> GetDeletionPredicate()
        {
            return ps => ps.Caption == _caption;
        }
    }
}