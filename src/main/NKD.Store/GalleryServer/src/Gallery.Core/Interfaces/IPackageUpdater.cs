using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageUpdater
    {
        void UpdateExistingPackage(Package packageToUpdate);
    }
}