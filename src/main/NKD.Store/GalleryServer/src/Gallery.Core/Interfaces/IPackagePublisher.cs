using Gallery.Core.Enums;

namespace Gallery.Core.Interfaces
{
    public interface IPackagePublisher
    {
        void PublishPackage(string key, string packageId, string packageVersion, PackageLogAction logActionForExistingPackage);
    }
}