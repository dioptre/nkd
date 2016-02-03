namespace Gallery.Core.Interfaces
{
    public interface ILatestVersionGetter<T>
        where T : class, IVersionable
    {
        T GetLatestVersion(string packageId);
    }
}