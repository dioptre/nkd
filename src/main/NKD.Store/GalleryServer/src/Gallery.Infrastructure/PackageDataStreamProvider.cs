using System;
using System.Data.Services;
using System.Data.Services.Providers;
using System.IO;
using System.Web;
using Gallery.Infrastructure.FeedModels;

namespace Gallery.Infrastructure
{
    public class PackageDataStreamProvider : IDataServiceStreamProvider
    {
        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public Stream GetReadStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException("Packages should be downloaded by requesting the Uri in the <content> tag.");
        }

        public Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            var publishedPackage = (PublishedPackage)entity;
            string absoluteUri = HttpContext.Current.Request.Url.AbsoluteUri;
            string siteRoot = absoluteUri.Substring(0, absoluteUri.IndexOf("/FeedService.svc/Packages"));
            string relativeUri = string.Format("{0}/Package/Download/{1}/{2}", siteRoot, publishedPackage.Id, publishedPackage.Version);
            return new Uri(relativeUri);
        }

        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            return "application/zip";
        }

        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        public Stream GetWriteStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public int StreamBufferSize
        {
            get
            {
                return 64000;
            }
        }
    }
}