using System;
using System.IO;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Gallery.Core;
using Gallery.Core.Interfaces;
using Gallery.DependencyResolution;

namespace Gallery.Server
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            DependencyRegistrar.EnsureDependenciesRegistered();
            LoggerConfigurator.ConfigureLogging();

            EnsureAppDataFolderExists();
            IoC.Resolver.Resolve<IDatabaseBootstrapper>().InitializeDatabase();
            IoC.Resolver.Resolve<IMapperBootstrapper>().RegisterMappings();
            RegisterWcfRoutes();

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }

        private void EnsureAppDataFolderExists()
        {
            var dataFolder = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data");
            IoC.Resolver.Resolve<IFileSystem>().CreateDirectoryIfNonexistent(dataFolder);
        }

        private static void RegisterWcfRoutes()
        {
            RouteTable.Routes.Add(new ServiceRoute("Packages", new WebServiceHostFactory(), typeof(PackageService)));
            RouteTable.Routes.Add(new ServiceRoute("Screenshots", new WebServiceHostFactory(), typeof(ScreenshotService)));
            RouteTable.Routes.Add(new ServiceRoute("PackageFiles", new WebServiceHostFactory(), typeof(PackageFileService)));
            RouteTable.Routes.Add(new ServiceRoute("PublishedPackages", new WebServiceHostFactory(), typeof(PackagePublishingService)));
            RouteTable.Routes.Add(new ServiceRoute("PackageLogs", new WebServiceHostFactory(), typeof(PackageLogService)));
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Download", // Route name
                "{controller}/{action}/{packageId}/{packageVersion}" // URL with parameters
                //new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }
    }
}