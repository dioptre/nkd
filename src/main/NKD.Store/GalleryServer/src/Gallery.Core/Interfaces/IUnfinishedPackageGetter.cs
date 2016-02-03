using System.Collections.Generic;
using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IUnfinishedPackageGetter
    {
        IEnumerable<Package> GetUnfinishedPackages(string userKey, IEnumerable<string> userPackageIds);
    }
}