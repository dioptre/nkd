using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Server
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class PackagePublishingService : ServiceBase
    {
        private readonly IPackagePublisher _packagePublisher;
        private readonly IPackageUnpublisher _packageUnpublisher;
        private readonly IServiceInputValidator _serviceInputValidator;

        public PackagePublishingService(IPackagePublisher packagePublisher, IPackageUnpublisher packageUnpublisher,
            IWebFaultExceptionCreator webFaultExceptionCreator, IServiceInputValidator serviceInputValidator, ILogger logger)
            : base(webFaultExceptionCreator, logger)
        {
            _packagePublisher = packagePublisher;
            _packageUnpublisher = packageUnpublisher;
            _serviceInputValidator = serviceInputValidator;
        }

        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, Method = "POST", RequestFormat = WebMessageFormat.Json)]
        public void Publish(string key, string id, string version)
        {
            const string defaultMessage = "An error occurred when publishing the Package";
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, id, version));
            Action action = () => _packagePublisher.PublishPackage(key, id, version, PackageLogAction.Update);
            ExecuteAction(action, defaultMessage);
        }

        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, Method = "POST", RequestFormat = WebMessageFormat.Json)]
        public void RePublish(string key, string id, string version)
        {
            const string defaultMessage = "An error occurred when re-publishing the Package";
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, id, version));
            Action action = () => _packagePublisher.PublishPackage(key, id, version, PackageLogAction.RePublish);
            ExecuteAction(action, defaultMessage);
        }

        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, Method = "POST", RequestFormat = WebMessageFormat.Json)]
        public void Unpublish(string key, string id, string version)
        {
            const string defaultMessage = "An error occurred when unpublishing the Package";
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, id, version));
            Action action = () => _packageUnpublisher.UnpublishPackage(key, id, version);
            ExecuteAction(action, defaultMessage);
        }
    }
}