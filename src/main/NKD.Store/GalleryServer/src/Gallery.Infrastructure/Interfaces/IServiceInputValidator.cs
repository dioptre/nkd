using System.Collections.Generic;
using Gallery.Core.Domain;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IServiceInputValidator
    {
        void ValidateUserApiKey(string key);
        void ValidateExternalUrl(string externalUrl);
        void ValidateKeysMatchInstance(string packageId, string packageVersion, Package packageInstance);
        void ValidateAllPackageKeys(string key, string packageId, string packageVersion);
        void ValidatePackageIds(IEnumerable<string> packageIds);
    }
}