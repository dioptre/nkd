using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Impl;
using NUnit.Framework;

namespace Gallery.UnitTests.AutoMapper
{
    [TestFixture]
    public class AutoMapperBootstrapperTester
    {
        [Test]
        public void ConfigurationShouldBeValid()
        {
            IMapperBootstrapper mapper = new AutoMapperBootstrapper(null, null);

            mapper.AssertConfigurationIsValid();
        }
    }
}