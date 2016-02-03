using System.IO;
using System.Security.Cryptography;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class Sha512HashingServiceProvider : IHashingServiceProvider
    {
        private readonly SHA512 _provider = SHA512.Create();

        public HashingAlgorithm HashingAlgorithm { get { return HashingAlgorithm.SHA512; } }

        public byte[] ComputeHash(Stream stream)
        {
            return _provider.ComputeHash(stream);
        }
    }
}