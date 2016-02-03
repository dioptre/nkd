using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageGetter
    {
        Package GetPackage(string key, string id, string version);
    }
}