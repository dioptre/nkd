using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.Plugins.IntegrationTests
{
    [TestFixture]
    public class NuPackPackageFactoryTester
    {
        private readonly IPackageFactory _packageFactory = new NuPackPackageFactory.NuPackPackageFactory();

        private const string NEW_NUPACK_FILE_NAME = "Ninject.2.0.1.0.nupkg";

        [Test]
        public void CreateNewFromFileShouldReturnPackageWhenGivenNewerPackageFile()
        {
            Package package = _packageFactory.CreateNewFromFile(NEW_NUPACK_FILE_NAME);

            Assert.IsNotNull(package, "Returned Package should not be null.");
            Assert.IsNotNullOrEmpty(package.Id, "Id should not be null or empty.");
            Assert.IsNotNullOrEmpty(package.Version, "Version should not be null or empty.");
        }
    }
}