using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class PackageAuthenticatorService : IPackageAuthenticatorService
    {
        private readonly IConfigSettings _configSettings;
        private readonly IDependencyResolver _dependencyResolver;

        public PackageAuthenticatorService(IConfigSettings configSettings, IDependencyResolver dependencyResolver)
        {
            _configSettings = configSettings;
            _dependencyResolver = dependencyResolver;
        }

        public IPackageAuthenticator RegisterPackageAuthenticator()
        {
            return _configSettings.AuthenticatePackageRequests ? (IPackageAuthenticator)_dependencyResolver.Resolve<PackageAuthenticator>()
                : _dependencyResolver.Resolve<NoOpPackageAuthenticator>();
        }
    }
}