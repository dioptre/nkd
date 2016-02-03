using System.IO;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    public class HashingServiceProviderTester : FileSystemTesterBase<IHashingServiceProvider>
    {
        [Test]
        public void ComputeHashShouldReturnComputedHashOfGivenFile()
        {
            byte[] computedHash;

            using (FileStream packageFile = File.OpenRead(EXISTING_FILE_NAME))
            {
                computedHash = Instance.ComputeHash(packageFile);
            }

            Assert.IsTrue(computedHash.Length > 0, "Empty computed hash returned.");
        }
    }
}