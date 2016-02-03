using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageUriValidator
    {
        void ValidatePackageUris(Package package);
    }
}