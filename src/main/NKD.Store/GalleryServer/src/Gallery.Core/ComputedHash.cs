using Gallery.Core.Enums;

namespace Gallery.Core
{
    public struct ComputedHash
    {
        public string ComputedHashCode;
        public HashingAlgorithm HashingAlgorithmUsed;

        public ComputedHash(string computedHashCode, HashingAlgorithm hashingAlgorithmUsed)
        {
            ComputedHashCode = computedHashCode;
            HashingAlgorithmUsed = hashingAlgorithmUsed;
        }
    }
}