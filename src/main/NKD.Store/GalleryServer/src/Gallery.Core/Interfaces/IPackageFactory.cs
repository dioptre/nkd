using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageFactory
    {
        Package CreateNewFromFile(string pathToPackageFile);
    }
}