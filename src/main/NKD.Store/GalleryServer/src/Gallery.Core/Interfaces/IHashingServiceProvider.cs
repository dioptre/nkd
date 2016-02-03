using System.IO;
using Gallery.Core.Enums;

namespace Gallery.Core.Interfaces
{
    public interface IHashingServiceProvider
    {
        HashingAlgorithm HashingAlgorithm { get; }
        byte[] ComputeHash(Stream stream);
    }
}