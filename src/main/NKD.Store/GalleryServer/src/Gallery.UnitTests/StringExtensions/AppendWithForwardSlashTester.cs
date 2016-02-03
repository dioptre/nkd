using System;
using NUnit.Framework;
using Gallery.Core.Extensions;

namespace Gallery.UnitTests.StringExtensions
{
    [TestFixture]
    public class AppendWithForwardSlashTester
    {
        [Test]
        public void ShouldThrowWhenFrontIsNull()
        {
            string nullString = null;

            TestDelegate methodThatShouldThrow = () => nullString.AppendWithForwardSlash("back");

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldThrowWhenBackIsNull()
        {
            string nullString = null;

            TestDelegate methodThatShouldThrow = () => "front".AppendWithForwardSlash(nullString);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Exception should have been thrown.");
        }

        [Test]
        public void ShouldReturnForwardSlashWhenBothStringsAreEmpty()
        {
            string emptyString = string.Empty;

            string appendedString = emptyString.AppendWithForwardSlash(emptyString);

            Assert.AreEqual("/", appendedString, "Appended string should have only been a forward slash.");
        }

        [Test]
        public void ShouldReturnStringJoinedBySingleForwardSlashWhenFrontEndsWithForwardSlash()
        {
            const string front = "front/";
            const string back = "back";
            const string expected = front + back;

            string appendedString = front.AppendWithForwardSlash(back);

            StringAssert.AreEqualIgnoringCase(expected, appendedString, "Returned string should have only had a single forward slash.");
        }

        [Test]
        public void ShouldReturnStringJoinedBySingleForwardSlashWhenBackBeginsWithForwardSlash()
        {
            const string front = "front";
            const string back = "/back";
            const string expected = front + back;

            string appendedString = front.AppendWithForwardSlash(back);

            StringAssert.AreEqualIgnoringCase(expected, appendedString, "Returned string should have only had a single forward slash.");
        }

        [Test]
        public void ShouldReturnStringJoinedByForwardSlashWhenNeitherStringContainsForwardSlash()
        {
            const string front = "front";
            const string back = "back";
            const string expected = front + "/" + back;

            string appendedString = front.AppendWithForwardSlash(back);

            StringAssert.AreEqualIgnoringCase(expected, appendedString, "Returned string should have had a forward slash.");
        }
    }
}