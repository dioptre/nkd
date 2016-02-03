using System;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.ServiceModel;
using Gallery.Core;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Server
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class FeedService : DataService<GalleryFeedContext>, IServiceProvider
    {
        private readonly Lazy<IDataServiceStreamProvider> _streamProvider;

        public FeedService()
        {
            _streamProvider = new Lazy<IDataServiceStreamProvider>(() => IoC.Resolver.Resolve<IDataServiceStreamProvider>());
        }

        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetPageSize("*", 100);
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(IDataServiceStreamProvider) ? _streamProvider.Value : null;
        }
    }
}