namespace Gallery.Core.Interfaces
{
    public interface IPackageUnpublisher
    {
        void UnpublishPackage(string key, string packageId, string packageVersion);
    }
}