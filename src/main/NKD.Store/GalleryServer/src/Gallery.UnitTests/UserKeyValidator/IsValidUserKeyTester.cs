using Gallery.Infrastructure.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.UserKeyValidator
{
    [TestFixture]
    public class IsValidUserKeyTester
    {
        private readonly IUserKeyValidator _userKeyValidator = new Infrastructure.Impl.UserKeyValidator();

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void ShouldNotAllowNullOrEmptyStrings(string key)
        {
            Assert.IsFalse(_userKeyValidator.IsValidUserKey(key), "API key of {0} should not validate.", key);
        }

        [TestCase("abcdefg12345", false)]
        [TestCase("F0B51D29-2EA2-477E-A9DC-3B175566193D", true)]
        public void ShouldOnlyValidateStringsThatAreGuids(string key, bool shouldValidate)
        {
            string not = shouldValidate ? "" : " not ";
            Assert.AreEqual(shouldValidate, _userKeyValidator.IsValidUserKey(key), "API key of {0} should{1}validate.", key, not);
        }
    }
}