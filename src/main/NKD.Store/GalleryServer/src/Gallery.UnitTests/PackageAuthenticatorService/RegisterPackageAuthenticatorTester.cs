using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Impl;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageAuthenticatorService
{
    [TestFixture]
    public class RegisterPackageAuthenticatorTester
    {
        private IPackageAuthenticatorService _packageAuthenticatorService;

        private Mock<IConfigSettings> _mockedConfigSettings;
        private Mock<IDependencyResolver> _mockedDependencyResolver;

        [SetUp]
        public void SetUp()
        {
            _mockedConfigSettings = new Mock<IConfigSettings>();
            _mockedDependencyResolver = new Mock<IDependencyResolver>();
            _packageAuthenticatorService = new Infrastructure.Impl.PackageAuthenticatorService(_mockedConfigSettings.Object, _mockedDependencyResolver.Object);

            _mockedDependencyResolver.Setup(dr => dr.Resolve<Infrastructure.Impl.PackageAuthenticator>())
                .Returns(new Infrastructure.Impl.PackageAuthenticator(null, null, null));
            _mockedDependencyResolver.Setup(dr => dr.Resolve<NoOpPackageAuthenticator>()).Returns(new NoOpPackageAuthenticator());
        }

        [Test]
        public void ShouldReturnNoOpAuthenticatorWhenSettingIsFalse()
        {
            _mockedConfigSettings.SetupGet(cs => cs.AuthenticatePackageRequests).Returns(false);

            IPackageAuthenticator packageAuthenticator = _packageAuthenticatorService.RegisterPackageAuthenticator();

            Assert.IsInstanceOf<NoOpPackageAuthenticator>(packageAuthenticator, "Returned authenticator should be of type {0}.",
                typeof(NoOpPackageAuthenticator).Name);
        }

        [Test]
        public void ShouldReturnPackageAuthenticatorWhenSettingIsTrue()
        {
            _mockedConfigSettings.SetupGet(cs => cs.AuthenticatePackageRequests).Returns(true);

            IPackageAuthenticator packageAuthenticator = _packageAuthenticatorService.RegisterPackageAuthenticator();

            Assert.IsInstanceOf<Infrastructure.Impl.PackageAuthenticator>(packageAuthenticator, "Returned authenticator should be of type {0}.",
                typeof(Infrastructure.Impl.PackageAuthenticator).Name);
        }

    }
}