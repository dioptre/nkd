using System.IO;
using System.Security.Cryptography;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class Md5CryptoServiceProvider : IHashingServiceProvider
    {
        private readonly MD5CryptoServiceProvider _provider = new MD5CryptoServiceProvider();

        public HashingAlgorithm HashingAlgorithm { get { return HashingAlgorithm.MD5; } }

        public byte[] ComputeHash(Stream stream)
        {
            return _provider.ComputeHash(stream);
        }
    }
}