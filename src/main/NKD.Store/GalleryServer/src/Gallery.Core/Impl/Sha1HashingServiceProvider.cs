using System.IO;
using System.Security.Cryptography;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class Sha1HashingServiceProvider : IHashingServiceProvider
    {
        private readonly SHA1 _provider = SHA1.Create();

        public HashingAlgorithm HashingAlgorithm { get { return HashingAlgorithm.SHA1; } }

        public byte[] ComputeHash(Stream stream)
        {
            return _provider.ComputeHash(stream);
        }
    }
}