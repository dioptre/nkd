using NUnit.Framework;
using Gallery.Core.Extensions;

namespace Gallery.UnitTests.StringExtensions
{
    public class NullifyEmptyTester
    {
        [Test]
        public void ShouldReturnNullWhenStringIsEmpty()
        {
            string emptyString = string.Empty;

            string result = emptyString.NullifyEmpty();

            Assert.IsNull(result, "Resultant string should have been null.");
        }

        [Test]
        public void ShouldReturnNullStringIsWhitespace()
        {
            string whitespaceString = "   ";

            string result = whitespaceString.NullifyEmpty();

            Assert.IsNull(result, "Resultant string should have been null.");
        }

        [Test]
        public void ShouldReturnOriginalValueWhenStringIsNotNullOrWhitespace()
        {
            string originalString = "test";

            string result = originalString.NullifyEmpty();

            Assert.AreEqual(originalString, result, "Result should match original string.");
        }
    }
}