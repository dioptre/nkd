namespace Gallery.Core.Interfaces
{
    public interface ILatestVersionChecker
    {
        bool IsLatestVersion(string packageId, string packageVersion);
    }
}