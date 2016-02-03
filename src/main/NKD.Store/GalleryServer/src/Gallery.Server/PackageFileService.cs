using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Server
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class PackageFileService : ServiceBase
    {
        private const string PACKAGE_FILE_EXTENSION = "nupkg";

        private readonly IPackageCreator _packageCreator;
        private readonly IServiceInputValidator _serviceInputValidator;
        private readonly IConfigSettings _configSettings;

        public PackageFileService(IPackageCreator packageCreator, IWebFaultExceptionCreator webFaultExceptionCreator,
            IServiceInputValidator serviceInputValidator, IConfigSettings configSettings, ILogger logger)
            : base(webFaultExceptionCreator, logger)
        {
            _packageCreator = packageCreator;
            _serviceInputValidator = serviceInputValidator;
            _configSettings = configSettings;
        }

        [WebInvoke(UriTemplate = "{key}/{fileExtension}", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        public Package Create(string key, string fileExtension, Stream packageFileStream)
        {
            return Create(key, packageFileStream, false);
        }

        [WebInvoke(UriTemplate = "{key}", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        public Package Replace(string key, Stream packageFileStream)
        {
            return Create(key, packageFileStream, true);
        }

        private Package Create(string key, Stream packageFileStream, bool isInPlaceUpdate)
        {
            ValidateInputs(() => _serviceInputValidator.ValidateUserApiKey(key));
            return ExecuteAction(() => _packageCreator.CreatePackage(key, packageFileStream, PACKAGE_FILE_EXTENSION, isInPlaceUpdate),
                "The Package could not be uploaded");
        }

        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Package CreateFromExternalUrl(string key, string fileExtension, string externalPackageUrl)
        {
            ValidateInputs(() => _serviceInputValidator.ValidateUserApiKey(key));
            ValidateInputs(() => _serviceInputValidator.ValidateExternalUrl(externalPackageUrl));

            Func<Package> action = () =>
            {
                using (var client = new ExtendedWebClient(_configSettings.ExternalPackageRequestTimeout))
                {
                    SetUserAgentForCodeplexCompliance(client);
                    byte[] bytes = client.DownloadData(externalPackageUrl);
                    return _packageCreator.CreatePackage(key, new MemoryStream(bytes), PACKAGE_FILE_EXTENSION, false, externalPackageUrl);
                }
            };
            return ExecuteAction(action, "The Package could not be created from the external source");
        }

        private static void SetUserAgentForCodeplexCompliance(WebClient client)
        {
            // Need to set the user agent to avoid the license acceptance dialog.
            var assemblyName = new AssemblyName(typeof (PackageFileService).Assembly.FullName);
            var version = assemblyName.Version;
            string userAgent = String.Format(CultureInfo.InvariantCulture, "Package-Installer/{0} ({1})", version, Environment.OSVersion);
            client.Headers.Add("user-agent", userAgent);
        }
    }
}