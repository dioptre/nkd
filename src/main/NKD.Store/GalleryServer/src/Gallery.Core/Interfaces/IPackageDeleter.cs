namespace Gallery.Core.Interfaces
{
    public interface IPackageDeleter
    {
        void DeletePackage(string packageId, string packageVersion);
    }
}