using Gallery.DependencyResolution;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    [TestFixture]
    public class DependencyRegistrarTester
    {
        [Test]
        public void EnsureDependenciesRegisteredShouldNotBomb()
        {
            DependencyRegistrar.EnsureDependenciesRegistered();
        }
    }
}