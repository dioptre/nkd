using System.IO;

namespace Gallery.Core.Interfaces
{
    public interface IPackageFileGetter
    {
        Stream GetPackageStream(string packageId, string packageVersion);
        string GetPackageDirectory(string packageId, string packageVersion);
        string GetPackagePath(string packageId, string packageVersion);
    }
}