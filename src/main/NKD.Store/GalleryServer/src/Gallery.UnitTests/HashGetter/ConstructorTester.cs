using System;
using NUnit.Framework;

namespace Gallery.UnitTests.HashGetter
{
    [TestFixture]
    public class ConstructorTester
    {
        [Test]
        public void ShouldThrowWhenGivenNullHashEncodingType()
        {
            Core.Enums.HashEncodingType nullHashEncodingType = null;

            TestDelegate methodThatShouldThrow = () => new Core.Impl.HashGetter(null, nullHashEncodingType);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Excpetion should have been thrown for null HashEncodingType.");
        }
    }
}