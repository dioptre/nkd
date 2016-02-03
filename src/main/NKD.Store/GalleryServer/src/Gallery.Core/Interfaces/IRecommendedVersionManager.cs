namespace Gallery.Core.Interfaces
{
    public interface IRecommendedVersionManager<T>
        where T : class, IVersionable
    {
        void SetLatestVersionAsRecommended(string packageId, bool shouldLogPackageAction);
    }
}