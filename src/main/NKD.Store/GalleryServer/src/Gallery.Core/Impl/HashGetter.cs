using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class HashGetter : IHashGetter
    {
        private readonly IHashingServiceProvider _hashingServiceProvider;
        private readonly HashEncodingType _hashEncodingType;

        public HashGetter(IHashingServiceProvider hashingServiceProvider, HashEncodingType hashEncodingType)
        {
            if (hashEncodingType == null)
            {
                throw new ArgumentNullException("hashEncodingType");
            }
            _hashingServiceProvider = hashingServiceProvider;
            _hashEncodingType = hashEncodingType;
        }

        public ComputedHash GetHashFromFile(Stream stream)
        {
            byte[] computedHash = _hashingServiceProvider.ComputeHash(stream);
            string hashString = GetHashString(computedHash);
            return new ComputedHash(hashString, _hashingServiceProvider.HashingAlgorithm);
        }

        private string GetHashString(byte[] computedHash)
        {
            string hashString;
            if (_hashEncodingType == HashEncodingType.Hex)
            {
                hashString = GetHexHashString(computedHash);
            }
            else if (_hashEncodingType == HashEncodingType.Base64)
            {
                hashString = Convert.ToBase64String(computedHash);
            }
            else
            {
                throw new NotSupportedException(string.Format("The HashEncodingType {0} is not supported.", _hashEncodingType));
            }
            return hashString;
        }

        private static string GetHexHashString(IEnumerable<byte> computedHash)
        {
            var hashStringBuilder = new StringBuilder();
            foreach (var @byte in computedHash)
            {
                hashStringBuilder.Append(@byte.ToString("x2"));
            }
            return hashStringBuilder.ToString();
        }
    }
}