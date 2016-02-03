using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class ServiceInputValidator : IServiceInputValidator
    {
        private readonly IUserKeyValidator _userKeyValidator;
        private readonly IPackageIdValidator _packageIdValidator;
        private readonly IPackageVersionValidator _packageVersionValidator;

        public ServiceInputValidator(IUserKeyValidator userKeyValidator, IPackageIdValidator packageIdValidator,
            IPackageVersionValidator packageVersionValidator)
        {
            _userKeyValidator = userKeyValidator;
            _packageIdValidator = packageIdValidator;
            _packageVersionValidator = packageVersionValidator;
        }

        public void ValidateUserApiKey(string key)
        {
            ValidateKey(key);
        }

        public void ValidateExternalUrl(string externalUrl)
        {
            const string exceptionMessage = "The URL for the package was not valid";
            var validSchemes = new[] {"http", "https", "ftp"};
            Uri checkedUri;

            if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out checkedUri))
            {
                throw new WebFaultException<string>(exceptionMessage, HttpStatusCode.BadRequest);
            }
            if (!validSchemes.Any(s => s == checkedUri.Scheme))
            {
                throw new WebFaultException<string>(exceptionMessage, HttpStatusCode.BadRequest);
            }
        }

        public void ValidateKeysMatchInstance(string packageId, string packageVersion, Package packageInstance)
        {
            if (packageId != packageInstance.Id)
            {
                throw new InvalidPackageIdException(packageInstance.Id);
            }
            if (packageVersion != packageInstance.Version)
            {
                throw new InvalidPackageVersionException(packageInstance.Version);
            }
        }

        public void ValidateAllPackageKeys(string key, string packageId, string packageVersion)
        {
            ValidateKey(key);
            ValidatePackageId(packageId);
            ValidatePackageVersion(packageVersion);
        }

        public void ValidatePackageIds(IEnumerable<string> packageIds)
        {
            if (packageIds != null)
            {
                foreach (var packageId in packageIds)
                {
                    ValidatePackageId(packageId);
                }
            }
        }

        private void ValidateKey(string key)
        {
            if (!_userKeyValidator.IsValidUserKey(key))
            {
                throw new InvalidUserKeyException(key);
            }
        }

        private void ValidatePackageId(string packageId)
        {
            if (!_packageIdValidator.IsValidPackageId(packageId))
            {
                throw new InvalidPackageIdException(packageId);
            }
        }

        private void ValidatePackageVersion(string packageVersion)
        {
            if (!_packageVersionValidator.IsValidPackageVersion(packageVersion))
            {
                throw new InvalidPackageVersionException(packageVersion);
            }
        }
    }
}