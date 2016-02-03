using System;
using NUnit.Framework;

namespace Gallery.UnitTests.HashEncodingType
{
    [TestFixture]
    public class FromNameTester
    {
        [TestCase("Hex")]
        [TestCase("hex")]
        [TestCase("HEX")]
        public void ShouldReturnHexWhenGivenHexString(string encodingType)
        {
            Core.Enums.HashEncodingType expectedHashEncodingType = Core.Enums.HashEncodingType.Hex;

            Core.Enums.HashEncodingType hashEncodingType = Core.Enums.HashEncodingType.FromName(encodingType);

            Assert.AreEqual(expectedHashEncodingType, hashEncodingType, "Incorrect HashEncodingType returned.");
        }

        [TestCase("Base64")]
        [TestCase("base64")]
        [TestCase("BASE64")]
        public void ShouldReturnBase64WhenGivenBase64String(string encodingType)
        {
            Core.Enums.HashEncodingType expectedHashEncodingType = Core.Enums.HashEncodingType.Base64;

            Core.Enums.HashEncodingType hashEncodingType = Core.Enums.HashEncodingType.FromName(encodingType);

            Assert.AreEqual(expectedHashEncodingType, hashEncodingType, "Incorrect HashEncodingType returned.");
        }

        [Test]
        public void ShouldThrowWhenGivenStringThatDoesNotMatchExistingEncodingTypes()
        {
            string typeThatDoesNotMatch = Guid.NewGuid().ToString();

            TestDelegate methodThatShouldThrow = () => Core.Enums.HashEncodingType.FromName(typeThatDoesNotMatch);

            Assert.Throws<InvalidOperationException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ShouldThrowWhenGivenNullEmptyOrWhiteSpaceString(string encodingType)
        {
            TestDelegate methodThatShouldThrow = () => Core.Enums.HashEncodingType.FromName(encodingType);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown.");
        }
    }
}