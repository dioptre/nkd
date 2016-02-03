using System;
using System.IO;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.HashGetter
{
    [TestFixture]
    public class GetHashFromFileTester
    {
        private Mock<IHashingServiceProvider> _mockedCryptoServiceProvider;

        [SetUp]
        public void SetUp()
        {
            _mockedCryptoServiceProvider = new Mock<IHashingServiceProvider>();
        }

        private IHashGetter GetHashGetter(Core.Enums.HashEncodingType hashEncodingType)
        {
            return new Core.Impl.HashGetter(_mockedCryptoServiceProvider.Object, hashEncodingType);
        }

        [Test]
        public void ShouldCallCryptoServiceProviderWithGivenStream()
        {
            IHashGetter hashGetter = GetHashGetter(Core.Enums.HashEncodingType.Hex);
            Stream expectedStream = Stream.Null;

            hashGetter.GetHashFromFile(expectedStream);

            _mockedCryptoServiceProvider.Verify(csp => csp.ComputeHash(expectedStream));
        }

        [Test]
        public void ShouldReturnHexHashWhenGivenHexHashEncodingType()
        {
            const string expectedHash = "0102030405060708090a";
            IHashGetter hashGetter = GetHashGetter(Core.Enums.HashEncodingType.Hex);
            var hash = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _mockedCryptoServiceProvider.Setup(csp => csp.ComputeHash(It.IsAny<Stream>())).Returns(hash);

            string hexHash = hashGetter.GetHashFromFile(null).ComputedHashCode;

            Assert.IsNotNullOrEmpty(hexHash, "Returned hash cannot be null or empty.");
            Assert.AreEqual(expectedHash, hexHash, "The string hash should have been in Hex.");
        }

        [Test]
        public void ShouldReturnBase64HashWhenGivenBase64HashEncodingType()
        {
            const string expectedHash = "AQIDBAUGBwgJCg==";
            IHashGetter hashGetter = GetHashGetter(Core.Enums.HashEncodingType.Base64);
            var hash = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _mockedCryptoServiceProvider.Setup(csp => csp.ComputeHash(It.IsAny<Stream>())).Returns(hash);

            string base64Hash = hashGetter.GetHashFromFile(null).ComputedHashCode;

            Assert.IsNotNullOrEmpty(base64Hash, "Returned hash cannot be null or empty.");
            Assert.AreEqual(expectedHash, base64Hash, "The string hash should have been in Base64.");
        }
    }
}