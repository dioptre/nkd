using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using System.Linq;
using Ninject.Extensions.Logging;

namespace Gallery.Server
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ScreenshotService : ServiceBase
    {
        private readonly IScreenshotDeleter _screenshotDeleter;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IRepository<Screenshot> _screenshotRepository;
        private readonly IServiceInputValidator _serviceInputValidator;

        public ScreenshotService(IScreenshotDeleter screenshotDeleter, IWebFaultExceptionCreator webFaultExceptionCreator,
            IPackageAuthenticator packageAuthenticator, IRepository<Screenshot> screenshotRepository,
            IServiceInputValidator serviceInputValidator, ILogger logger)
            : base(webFaultExceptionCreator, logger)
        {
            _screenshotDeleter = screenshotDeleter;
            _screenshotRepository = screenshotRepository;
            _serviceInputValidator = serviceInputValidator;
            _packageAuthenticator = packageAuthenticator;
        }

        [WebInvoke(UriTemplate = "{key}", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public Screenshot Create(string key, Screenshot instance)
        {
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, instance.PackageId, instance.PackageVersion));
            Func<Screenshot> actionToExecute = () =>
            {
                _packageAuthenticator.EnsureKeyCanAccessPackage(key, instance.PackageId, instance.PackageVersion);
                return _screenshotRepository.Create(instance);
            };
            return ExecuteAction(actionToExecute, "The Screenshot could not be created");
        }

        [WebInvoke(UriTemplate = "{key}/{id}", Method = "DELETE")]
        public void Delete(string key, string id)
        {
            ValidateInputs(() => _serviceInputValidator.ValidateUserApiKey(key));
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new WebFaultException<string>("A Screenshot ID is required.", HttpStatusCode.BadRequest);
            }
            int idOfScreenshotToDelete;
            if (!int.TryParse(id, out idOfScreenshotToDelete))
            {
                throw new WebFaultException<string>(string.Format("A malformed Screenshot ID was given. '{0}' is not valid.", id), HttpStatusCode.BadRequest);
            }
            Screenshot screenshotToDelete = _screenshotRepository.Collection.SingleOrDefault(s => s.Id == idOfScreenshotToDelete);
            if (screenshotToDelete == null)
            {
                throw new WebFaultException<string>("The specified Screenshot does not exist.", HttpStatusCode.BadRequest);
            }
            Action deleteScreenshotAction = () =>
            {
                _packageAuthenticator.EnsureKeyCanAccessPackage(key, screenshotToDelete.PackageId, screenshotToDelete.PackageVersion);
                _screenshotDeleter.DeleteScreenshot(idOfScreenshotToDelete);
            };
            ExecuteAction(deleteScreenshotAction, "The Screenshot could not be deleted");
        }
    }
}