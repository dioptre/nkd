using System.IO;
using Gallery.Core;
using Gallery.Core.Interfaces;
using log4net.Config;

namespace Gallery.DependencyResolution
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogging()
        {
            string configFullPath = Path.Combine(IoC.Resolver.Resolve<IHttpRuntime>().AppDomainAppPath, "log4net.config");
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configFullPath));
        }
    }
}