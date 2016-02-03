using System.IO;

namespace Gallery.Core.Interfaces
{
    public interface IHashGetter
    {
        ComputedHash GetHashFromFile(Stream stream);
    }
}