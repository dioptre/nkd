using NUnit.Framework;

namespace Gallery.UnitTests.HashEncodingType
{
    [TestFixture]
    public class ToStringTester
    {
        [Test]
        public void ShouldReturnNameOfGivenEncodingType()
        {
            const string expectedToStringValue = "Base64";

            Core.Enums.HashEncodingType type = Core.Enums.HashEncodingType.Base64;

            Assert.AreEqual(expectedToStringValue, type.ToString(), "Incorrect value for ToString returned.");
        }
    }
}