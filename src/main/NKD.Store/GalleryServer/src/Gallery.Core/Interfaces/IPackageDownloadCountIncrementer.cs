namespace Gallery.Core.Interfaces
{
    public interface IPackageDownloadCountIncrementer
    {
        void Increment(string packageId, string packageVersion);
    }
}