using Gallery.Core.Domain;

namespace Gallery.Core.Interfaces
{
    public interface IPackageMapper<in T>
    {
        Package Map(T objectToMapFrom);
    }
}