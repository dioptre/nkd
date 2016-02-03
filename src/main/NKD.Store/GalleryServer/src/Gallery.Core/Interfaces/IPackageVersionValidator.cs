namespace Gallery.Core.Interfaces
{
    public interface IPackageVersionValidator
    {
        bool IsValidPackageVersion(string packageVersion);
    }
}