using System;
using Gallery.Core;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    public class RecommendedVersionManagerTester : IntegrationTesterBase<IRecommendedVersionManager<Package>>
    {

        public RecommendedVersionManagerTester()
            : base(true)
        { }

        [TearDown]
        public void TestTearDown()
        {
            IoC.Resolver.Resolve<IRepository<Package>>().DeleteMany(p => true);
        }

        [Test]
        public void ShouldNotThrowAnException()
        {
            Package existingPackage = new Package { Id = Guid.NewGuid().ToString(), Version  = "1.0.5.3",
                Created = DateTime.Now, LastUpdated = DateTime.Today };
            IoC.Resolver.Resolve<IRepository<Package>>().Create(existingPackage);

            Instance.SetLatestVersionAsRecommended(existingPackage.Id, false);
        }
    }
}