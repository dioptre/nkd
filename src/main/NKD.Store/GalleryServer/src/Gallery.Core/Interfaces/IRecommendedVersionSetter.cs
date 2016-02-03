namespace Gallery.Core.Interfaces
{
    public interface IRecommendedVersionSetter<T>
        where T : class, IVersionable
    {
        void SetAsRecommendedVersion(T versionable, bool shouldLogPackageAction);
    }
}