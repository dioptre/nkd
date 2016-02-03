namespace Gallery.Core.Interfaces
{
    public interface ILatestVersionUpdater<TVersionable>
        where TVersionable : class, IVersionable
    {
        void SetLatestVersionFlagsOfOtherVersionablesWithSameId(TVersionable versionable);
    }
}