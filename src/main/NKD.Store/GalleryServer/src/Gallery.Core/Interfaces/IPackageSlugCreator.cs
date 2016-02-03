namespace Gallery.Core.Interfaces
{
    public interface IPackageSlugCreator
    {
        string CreateSlug(string packageId, string packageVersion);
    }
}