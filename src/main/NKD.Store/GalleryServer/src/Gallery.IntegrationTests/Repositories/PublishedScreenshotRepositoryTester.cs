using System;
using Gallery.Infrastructure.FeedModels;
using Gallery.IntegrationTests.Helpers;

namespace Gallery.IntegrationTests.Repositories
{
    public class PublishedScreenshotRepositoryTester : RepositoryTesterBase<PublishedScreenshot>
    {
        private readonly string _caption = string.Format("Integration Test - {0}", Guid.NewGuid());

        protected override PublishedScreenshot GetObjectToCreate()
        {
            PublishedPackage existingPackage = PackageTestHelpers.GetExistingPublishedPackage();
            return new PublishedScreenshot
            {
                Caption = _caption,
                PublishedPackageId = existingPackage.Id,
                PublishedPackageVersion = existingPackage.Version
            };
        }

        protected override Func<PublishedScreenshot, bool> GetDeletionPredicate()
        {
            return ps => ps.Caption == _caption;
        }
    }
}