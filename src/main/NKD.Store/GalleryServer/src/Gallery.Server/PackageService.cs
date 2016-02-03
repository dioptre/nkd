using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Server {
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class PackageService : ServiceBase {
        private readonly IPackageDeleter _packageDeleter;
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IPackageUpdater _packageUpdater;
        private readonly IServiceInputValidator _serviceInputValidator;
        private readonly IPackageGetter _packageGetter;
        private readonly IPackageRatingUpdater _packageRatingUpdater;
        private readonly IRatingAuthorizer _ratingAuthorizer;
        private readonly IUnfinishedPackageGetter _unfinishedPackageGetter;

        public PackageService(IPackageDeleter packageDeleter, IPackageAuthenticator packageAuthenticator,
            IPackageUpdater packageUpdater, IWebFaultExceptionCreator webFaultExceptionCreator, IServiceInputValidator serviceInputValidator, ILogger logger,
            IPackageGetter packageGetter, IPackageRatingUpdater packageRatingUpdater, IRatingAuthorizer ratingAuthorizer,
            IUnfinishedPackageGetter unfinishedPackageGetter)
            : base(webFaultExceptionCreator, logger) {
            _packageUpdater = packageUpdater;
            _serviceInputValidator = serviceInputValidator;
            _packageDeleter = packageDeleter;
            _packageAuthenticator = packageAuthenticator;
            _packageGetter = packageGetter;
            _packageRatingUpdater = packageRatingUpdater;
            _ratingAuthorizer = ratingAuthorizer;
            _unfinishedPackageGetter = unfinishedPackageGetter;
        }

        [WebInvoke(UriTemplate = "{key}/{id}/{version}", Method = "PUT", ResponseFormat = WebMessageFormat.Json)]
        public void Update(string key, string id, string version, Package instance)
        {
            Action validateInputsAction = () =>
            {
                _serviceInputValidator.ValidateAllPackageKeys(key, id, version);
                _serviceInputValidator.ValidateKeysMatchInstance(id, version, instance);
            };
            ValidateInputs(validateInputsAction);
            Action updateAction = () => {
                _packageAuthenticator.EnsureKeyCanAccessPackage(key, id, version);
                _packageUpdater.UpdateExistingPackage(instance);
            };
            ExecuteAction(updateAction, "The Package could not be updated");
        }

        [WebInvoke(UriTemplate = "{key}/{id}/{version}", Method = "DELETE")]
        public void Delete(string key, string id, string version) {
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, id, version));
            Action actionToExecute = () => {
                _packageAuthenticator.EnsureKeyCanAccessPackage(key, id, version);
                _packageDeleter.DeletePackage(id, version);
            };
            ExecuteAction(actionToExecute, "The Package could not be deleted");
        }

        [WebGet(UriTemplate = "{key}/{id}/{version}", ResponseFormat = WebMessageFormat.Json)]
        public Package Get(string key, string id, string version) {
            ValidateInputs(() => _serviceInputValidator.ValidateAllPackageKeys(key, id, version));
            return ExecuteAction(() => _packageGetter.GetPackage(key, id, version), "Could not retrieve the selected Package");
        }

        [WebInvoke(UriTemplate = "{key}", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public IEnumerable<Package> GetUnfinished(string key, IEnumerable<string> packageIds)
        {
            ValidateInputs(() =>
            {
                _serviceInputValidator.ValidateUserApiKey(key);
                _serviceInputValidator.ValidatePackageIds(packageIds);
            });
            string errorMessagePrefix = string.Format("Could not check for unfinished packages");
            return ExecuteAction(() => _unfinishedPackageGetter.GetUnfinishedPackages(key, packageIds), errorMessagePrefix);
        }

        [WebInvoke(UriTemplate = "UpdatePackageRatings/{nonce}", Method = "PUT")]
        public void UpdatePackageRatings(IEnumerable<PackageVersionRatings> updatedPackageRatingAggregates, string nonce)
        {
            Action actionToExecute = () =>
            {
                _ratingAuthorizer.ValidateNonce(nonce);
                _packageRatingUpdater.UpdatePackageRatings(updatedPackageRatingAggregates);
            };
            ExecuteAction(actionToExecute, "Package ratings could not be updated");
        }
    }
}
