using System.Net;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Microsoft.Http;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageAuthenticator
{
    [TestFixture]
    public abstract class PackageTesterBase
    {
        protected IPackageAuthenticator PackageAuthenticator;

        protected Mock<IHttpClientAdapter> MockedHttpClientAdapter;
        protected Mock<IHttpClient> MockedHttpClient;
        protected Mock<IConfigSettings> MockedConfigSettings;

        protected HttpResponseMessage OkHttpResponse;

        [SetUp]
        public void SetUp()
        {
            MockedHttpClientAdapter = new Mock<IHttpClientAdapter>();
            MockedHttpClient = new Mock<IHttpClient>();
            MockedConfigSettings = new Mock<IConfigSettings>();

            PackageAuthenticator = new Infrastructure.Impl.PackageAuthenticator(MockedHttpClientAdapter.Object, MockedConfigSettings.Object,
                new Mock<ILogger>().Object);

            OkHttpResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = HttpContent.Create("true") };
        }
    }
}