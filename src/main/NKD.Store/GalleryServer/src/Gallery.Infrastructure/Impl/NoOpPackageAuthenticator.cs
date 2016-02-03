using System.Collections.Generic;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class NoOpPackageAuthenticator : IPackageAuthenticator
    {
        public void EnsureKeyCanAccessPackage(string key, string packageId, string packageVersion)
        { }

        public void EnsureKeyCanAccessPackages(IEnumerable<string> packageIds, string key)
        { }

        public void EnsureKeyExists(string key)
        { }
    }
}