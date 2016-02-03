using System.Data.Services.Providers;
using Gallery.Core;
using Gallery.Core.Domain;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Impl;
using Gallery.Infrastructure.Interfaces;
using Gallery.Infrastructure.Repositories;
using IFileSystem = Gallery.Core.Interfaces.IFileSystem;
using PackageAuthenticatorService = Gallery.Infrastructure.Impl.PackageAuthenticatorService;

namespace Gallery.DependencyResolution
{
    public static class DependencyRegistrar
    {
        private static bool _alreadyRegistered;

        public static void EnsureDependenciesRegistered()
        {
            if (_alreadyRegistered)
            {
                return;
            }
            IoC.Resolver = new NinjectDependencyResolver();
            ForceReregistration();
            _alreadyRegistered = true;
        }

        private static void ForceReregistration()
        {
            IoC.Resolver.Register<IConfigSettings, ConfigSettings>();
            IoC.Resolver.Register<IConfigurationManager, SystemConfigurationManager>();
            IoC.Resolver.Register<IDatabaseBootstrapper, MigrationBootstrapper>();
            IoC.Resolver.Register<IDatabaseContext, GalleryFeedEntities>();
            IoC.Resolver.Register<IDataServiceStreamProvider, PackageDataStreamProvider>();
            IoC.Resolver.Register<IDateTime, SystemDateTime>();
            IoC.Resolver.Register<IPackageLogEntryCreator, PackageLogEntryCreator>();
            IoC.Resolver.Register<IDependencyStringFactory, DependencyStringFactory>();
            IoC.Resolver.Register<IFileSystem, WindowsFileSystem>();
            IoC.Resolver.Register<IGalleryDetailsUriCreator, GalleryDetailsUriCreator>();
            IoC.Resolver.Register<IGalleryUriValidator, GalleryUriValidator>();
            IoC.Resolver.Register<IGuid, SystemGuid>();
            IoC.Resolver.Register<IHttpClient, SystemHttpClient>();
            IoC.Resolver.Register<IHttpClientAdapter, HttpClientAdapter>();
            IoC.Resolver.Register<IHttpRuntime, SystemHttpRuntime>();
            IoC.Resolver.Register<ILatestVersionGetter<Package>, LatestVersionGetter<Package>>();
            IoC.Resolver.Register<ILatestVersionGetter<PublishedPackage>, LatestVersionGetter<PublishedPackage>>();
            IoC.Resolver.Register<IRecommendedVersionSetter<Package>, RecommendedVersionSetter<Package>>();
            IoC.Resolver.Register<IRecommendedVersionSetter<PublishedPackage>, RecommendedVersionSetter<PublishedPackage>>();
            IoC.Resolver.Register<ILatestVersionChecker, LatestVersionChecker>();
            IoC.Resolver.Register<ILatestVersionUpdater<Package>, LatestVersionUpdater<Package>>();
            IoC.Resolver.Register<ILatestVersionUpdater<PublishedPackage>, LatestVersionUpdater<PublishedPackage>>();
            IoC.Resolver.Register<IMapper, AutoMapper>();
            IoC.Resolver.Register<IMapperBootstrapper, AutoMapperBootstrapper>();
            IoC.Resolver.Register<IHashGetter, HashGetter>();
            IoC.Resolver.Register<IPackageAuthenticatorService, PackageAuthenticatorService>();
            IoC.Resolver.Register<IPackageCreator, PackageCreator>();
            IoC.Resolver.Register<IPackageDataAggregateUpdater, PackageDataAggregateUpdater>();
            IoC.Resolver.Register<IPackageDeleter, PackageDeleter>();
            IoC.Resolver.Register<IPackageDownloadCountIncrementer, PackageDownloadCountIncrementer>();
            IoC.Resolver.Register<IPackageFileGetter, PackageFileGetter>();
            IoC.Resolver.Register<IPackageGetter, PackageGetter>();
            IoC.Resolver.Register<IPackageIdValidator, PackageIdValidator>();
            IoC.Resolver.Register<IPackagePublisher, PackagePublisher>();
            IoC.Resolver.Register<IPackageUnpublisher, PackageUnpublisher>();
            IoC.Resolver.Register<IPackageRatingCalculator, PackageRatingCalculator>();
            IoC.Resolver.Register<IPackageRatingUpdater, PackageRatingUpdater>();
            IoC.Resolver.Register<IRecommendedVersionManager<Package>, RecommendedVersionManager<Package>>();
            IoC.Resolver.Register<IRecommendedVersionManager<PublishedPackage>, RecommendedVersionManager<PublishedPackage>>();
            IoC.Resolver.Register<IPackageReportAbuseUriCreator, PackageReportAbuseUriCreator>();
            IoC.Resolver.Register<IPackageSlugCreator, PackageSlugCreator>();
            IoC.Resolver.Register<IPackageUpdater, PackageUpdater>();
            IoC.Resolver.Register<IPackageUriValidator, PackageUriValidator>();
            IoC.Resolver.Register<IPackageVersionValidator, PackageVersionValidator>();
            IoC.Resolver.Register<IRatingAuthorizer, RatingAuthorizer>();
            IoC.Resolver.Register<IScreenshotDeleter, ScreenshotDeleter>();
            IoC.Resolver.Register<IServiceInputValidator, ServiceInputValidator>();
            IoC.Resolver.Register<ITaskScheduler, SequentialTaskScheduler>();
            IoC.Resolver.Register<IUnfinishedPackageGetter, UnfinishedPackageGetter>();
            IoC.Resolver.Register<IUserKeyValidator, UserKeyValidator>();
            IoC.Resolver.Register<IWebFaultExceptionCreator, WebFaultExceptionCreator>();
            IoC.Resolver.Register(() => IoC.Resolver);

            IoC.Resolver.Register(IoC.Resolver.Resolve<HashEncodingTypeService>().GetHashEncodingTypeToUse);
            IoC.Resolver.Register(IoC.Resolver.Resolve<HashingProviderService>().RegisterHashingProviderImplementer);
            IoC.Resolver.Register(IoC.Resolver.Resolve<IPackageAuthenticatorService>().RegisterPackageAuthenticator);
            IoC.Resolver.Register(IoC.Resolver.Resolve<PluginService>().FindPluginImplementation<IPackageFactory>);

            RegisterRepositories();
        }

        private static void RegisterRepositories()
        {
            IoC.Resolver.Register<IRepository<PackageLogEntry>, PackageLogEntryRepository>();
            IoC.Resolver.Register<IRepository<Package>, PackageRepository>();
            IoC.Resolver.Register<IRepository<PublishedPackage>, PublishedPackageRepository>();
            IoC.Resolver.Register<IRepository<PublishedScreenshot>, PublishedScreenshotRepository>();
            IoC.Resolver.Register<IRepository<Screenshot>, ScreenshotRepository>();
            IoC.Resolver.Register<IRepository<PackageDataAggregate>, PackageDataAggregateRepository>();
            IoC.Resolver.Register<IRepository<Dependency>, DependencyRepository>();
        }
    }
}
