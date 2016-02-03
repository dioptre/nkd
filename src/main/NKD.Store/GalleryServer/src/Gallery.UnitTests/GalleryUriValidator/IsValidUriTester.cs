using System;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.GalleryUriValidator
{
    [TestFixture]
    public class IsValidUriTester
    {
        private IGalleryUriValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new Core.Impl.GalleryUriValidator();
        }

        [TestCase("foo", true)]
        [TestCase("http://foo.bar", true)]
        [TestCase("https://foo.bar", true)]
        [TestCase("javascript:alert('foo')", false)]
        [TestCase("ftp://foo.bat", false)]
        public void ShouldOnlyAllowRelativeOrAbsoluteUrisThatAreHttpOrHttpsUris(string uri, bool expectedResult)
        {
            bool isValidUri = _validator.IsValidUri(uri);

            Assert.AreEqual(expectedResult, isValidUri);
        }

        [TestCase("http://foo.bar", true)]
        [TestCase("foo", false)]
        public void ShouldOnlyAcceptAbsoluteUrisWhenAbsoluteUriKindGiven(string uri, bool expectedResult)
        {
            bool isValidUri = _validator.IsValidUri(uri, UriKind.Absolute);

            Assert.AreEqual(expectedResult, isValidUri);
        }

        [TestCase("http://foo.bar", false)]
        [TestCase("foo", true)]
        public void ShouldOnlyAcceptRelativeUrisWhenRelativeUriKindGiven(string uri, bool expectedResult)
        {
            bool isValidUri = _validator.IsValidUri(uri, UriKind.Relative);

            Assert.AreEqual(expectedResult, isValidUri);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ShouldAllowNullEmptyOrWhitespaceValuesRegardlessOfUriSetting(string uri)
        {
            Assert.IsTrue(_validator.IsValidUri(uri));
            Assert.IsTrue(_validator.IsValidUri(uri, UriKind.Absolute));
            Assert.IsTrue(_validator.IsValidUri(uri, UriKind.Relative));
        }
    }
}