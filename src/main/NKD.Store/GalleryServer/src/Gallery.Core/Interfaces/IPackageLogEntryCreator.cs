using Gallery.Core.Enums;

namespace Gallery.Core.Interfaces
{
    public interface IPackageLogEntryCreator
    {
        void Create(string packageId, string packageVersion, PackageLogAction action);
    }
}