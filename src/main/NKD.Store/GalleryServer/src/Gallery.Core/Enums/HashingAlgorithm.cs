using System.Collections.Generic;
using System.Linq;

namespace Gallery.Core.Enums
{
    public sealed class HashingAlgorithm
    {
        public static readonly HashingAlgorithm MD5 = new HashingAlgorithm("MD5");
        public static readonly HashingAlgorithm SHA1 = new HashingAlgorithm("SHA1");
        public static readonly HashingAlgorithm SHA512 = new HashingAlgorithm("SHA512");

        public readonly string Name;

        private HashingAlgorithm(string name)
        {
            Name = name;
        }

        private static readonly List<HashingAlgorithm> _algorithms = new List<HashingAlgorithm> {MD5, SHA1, SHA512};
        public static HashingAlgorithm FromName(string name)
        {
            return _algorithms.Single(a => string.Compare(a.Name, name, true) == 0);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}