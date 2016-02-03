using System.IO;
using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageCreator
    {
        Package CreatePackage(string key, Stream packageFileStream, string fileExtension, bool isInPlaceUpdate, string externalPackageUri = null);
    }
}