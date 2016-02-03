using System;
using System.Data.Entity.Validation;
using System.Linq;
using Gallery.Core.Domain;
using NUnit.Framework;

namespace Gallery.IntegrationTests.Repositories
{
    public class PackageRepositoryTester : RepositoryTesterBase<Package>
    {
        private static readonly string _id = string.Format("Id-{0}", Guid.NewGuid());
        private static readonly string _version = string.Format("Version-{0}", Guid.NewGuid());

        protected override Package GetObjectToCreate()
        {
            return new Package
            {
                Id = _id,
                Version = _version,
                Created = DateTime.Today,
                LastUpdated = DateTime.Now
            };
        }

        private Package CreatePackageInDatabase(string id)
        {
            Package package = GetObjectToCreate();
            package.Id = id;
            Instance.Create(package);
            return package;
        }

        protected override Func<Package, bool> GetDeletionPredicate()
        {
            return p => p.Id == _id && p.Version == _version;
        }

        [Test]
        public void UpdatingMultiplePackagesShouldWork()
        {
            Package packageA = CreatePackageInDatabase("A");
            Package packageB = CreatePackageInDatabase("B");
            Package packageC = CreatePackageInDatabase("C");

            packageA.ExternalPackageUrl = "http://foo.com";
            packageB.Title = "Some Title";
            packageC.Language = "Spanish";

            Instance.Update(new[] {packageA, packageB, packageC});

            Package updatedPackageA = Instance.Collection.Where(p => p.Id == "A").Single();
            Package updatedPackageB = Instance.Collection.Where(p => p.Id == "B").Single();
            Package updatedPackageC = Instance.Collection.Where(p => p.Id == "C").Single();

            Assert.AreEqual(packageA.ExternalPackageUrl, updatedPackageA.ExternalPackageUrl, "Package A's ExternalPackageUrl was not updated.");
            Assert.AreEqual(packageB.Title, updatedPackageB.Title, "Package B's Title was not updated.");
            Assert.AreEqual(packageC.Language, updatedPackageC.Language, "Package C's Language was not updated.");
        }

        [Test]
        public void ShouldBeAbleToCreatePackageWithStringFieldGreaterThan128Characters()
        {
            const string summary = "This is longer than 128 characters. I wonder if it will be able to be persisted? Only need a few more characters to reach the length.";

            Package packageToCreate = GetObjectToCreate();
            packageToCreate.Summary = summary;

            try
            {
                Instance.Create(packageToCreate);
            }
            catch (DbEntityValidationException ex)
            {
                Assert.Fail("DbEntityValidationException was thrown and left the following error messages: " +
                    string.Join("|", ex.EntityValidationErrors.SelectMany(eve => eve.ValidationErrors).Select(e => e.ErrorMessage)));
            }
        }
    }
}