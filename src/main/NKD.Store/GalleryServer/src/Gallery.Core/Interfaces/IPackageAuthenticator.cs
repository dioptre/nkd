using System.Collections.Generic;

namespace Gallery.Core.Interfaces
{
    public interface IPackageAuthenticator
    {
        void EnsureKeyCanAccessPackage(string key, string packageId, string packageVersion);
        void EnsureKeyCanAccessPackages(IEnumerable<string> packageIds, string key);
    }
}