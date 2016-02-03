using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.PackageIdValidator
{
    [TestFixture]
    public class IsValidPackageIdTester
    {
        private readonly IPackageIdValidator _packageIdValidator = new Core.Impl.PackageIdValidator();

        private void AssertPackageCannotStartWithCharacter(char characterToStartWith)
        {
            string packageId = characterToStartWith + "abc";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsFalse(isValidPackageId, "Package ID starting with {0} should not be considered valid.", characterToStartWith);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void ShouldNotAllowNullOrEmptyStrings(string packageId)
        {
            Assert.IsFalse(_packageIdValidator.IsValidPackageId(packageId), "PackageId of {0} should not validate.", packageId);
        }


        [TestCase("abcdefgh")]
        [TestCase("1234567")]
        [TestCase("1234abd")]
        [TestCase("abd1234")]
        public void ShouldAllowPackageIdsComposedOnlyOfLettersAndNumbers(string packageId)
        {
            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package IDs containing solely letters and numbers should be allowed.");
        }

        [TestCase("abcdefgh")]
        [TestCase("ABCDEFGH")]
        [TestCase("ABCdefGH")]
        public void ShouldAllowPackageIdWithLettersRegardlessOfCase(string packageId)
        {
            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package IDs containing solely letters should be allowed regardless of case.");
        }

        [Test]
        public void ShouldNotAllowSpaces()
        {
            const string packageId = "abc 123";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsFalse(isValidPackageId, "Package ID with spaces should not be validated.");
        }

        [TestCase('\'')]
        [TestCase('\\')]
        [TestCase('/')]
        [TestCase('&')]
        [TestCase('İ')]
        public void ShouldNotAllowSpecialCharacters(char specialCharacter)
        {
            string packageId = "abc" + specialCharacter + "123";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsFalse(isValidPackageId, "Package ID '{0}' should be considered invalid since it contains a special character ({1}).",
                packageId, specialCharacter);
        }

        [TestCase('A')]
        [TestCase('x')]
        [TestCase('I')]
        public void PackageIdShouldBeAbleToStartWithLetter(char characterToStartWith)
        {
            string packageId = characterToStartWith + "JFDKLFJLKWE";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package ID starting with a letter should be considered valid.");
        }

        [TestCase("0")]
        [TestCase("5")]
        [TestCase("17")]
        public void PackageIdShouldBeAbleToStartWithNumbers(string numberToStartWith)
        {
            string packageId = numberToStartWith + "JFDKLFJLKWE";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package ID starting with a number should be considered valid.");
        }

        [TestCase('A')]
        [TestCase('x')]
        [TestCase('I')]
        public void PackageIdShouldBeAbleToEndWithLetter(char characterToEndWith)
        {
            string packageId = "JFDKLFJLKWE" + characterToEndWith;

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package ID ending with a letter should be considered valid.");
        }

        [TestCase("0")]
        [TestCase("5")]
        [TestCase("17")]
        public void PackageIdShouldBeAbleToEndWithNumbers(string numberToEndWith)
        {
            string packageId = "JFDKLFJLKWE" + numberToEndWith;

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package ID ending with a number should be considered valid.");
        }

        [Test]
        public void ShouldNotAllowPackageIdToStartWithUnderscore()
        {
            AssertPackageCannotStartWithCharacter('_');
        }

        [Test]
        public void ShouldNotAllowPackageIdToStartWithDash()
        {
            AssertPackageCannotStartWithCharacter('-');
        }

        [Test]
        public void ShouldNotAllowPackageIdToStartWithDot()
        {
            AssertPackageCannotStartWithCharacter('.');
        }

        [Test]
        public void ShouldReturnTrueWhenPackageIdContainsADash()
        {
            AssertPackageIdCanContainCharacter('-');
        }

        [Test]
        public void ShouldReturnTrueWhenPackageIdContainsADot()
        {
            AssertPackageIdCanContainCharacter('.');
        }

        [Test]
        public void ShouldReturnTrueWhenPackageIdContainsAnUnderscore()
        {
            AssertPackageIdCanContainCharacter('_');
        }

        [Test]
        public void ShouldReturnFalseWhenContainingTwoConsecutiveDashes()
        {
            AssertPackageIdCannotContainTwoConsecutiveCharacters('-');
        }

        [Test]
        public void ShouldReturnFalseWhenContainingTwoConsecutiveDots()
        {
            AssertPackageIdCannotContainTwoConsecutiveCharacters('.');
        }

        [Test]
        public void ShouldReturnFalseWhenContainingTwoConsecutiveUnderscores()
        {
            AssertPackageIdCannotContainTwoConsecutiveCharacters('_');
        }

        [Test]
        public void ShouldReturnFalseWhenPackageIdEndsWithDash()
        {
            AssertPackageIdCannotEndWithCharacter('-');
        }

        [Test]
        public void ShouldReturnFalseWhenPackageIdEndsWithDot()
        {
            AssertPackageIdCannotEndWithCharacter('.');
        }

        [Test]
        public void ShouldReturnFalseWhenPackageIdEndsWithUnderscore()
        {
            AssertPackageIdCannotEndWithCharacter('_');
        }

        private void AssertPackageIdCannotEndWithCharacter(char characterToEndWith)
        {
            string packageId = "abc" + characterToEndWith;

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsFalse(isValidPackageId, "Package ID ending with {0} should not be considered valid.", characterToEndWith);

        }

        private void AssertPackageIdCannotContainTwoConsecutiveCharacters(char characterToContain)
        {
            string packageId = "abc" + characterToContain + characterToContain + "123";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsFalse(isValidPackageId, "Package ID containing two consecutive {0} characters should not be considered valid.",
                characterToContain);
        }

        private void AssertPackageIdCanContainCharacter(char characterToContain)
        {
            string packageId = "abc" + characterToContain + "123";

            bool isValidPackageId = _packageIdValidator.IsValidPackageId(packageId);

            Assert.IsTrue(isValidPackageId, "Package ID containing {0} should be considered valid.", characterToContain);
        }
    }
}