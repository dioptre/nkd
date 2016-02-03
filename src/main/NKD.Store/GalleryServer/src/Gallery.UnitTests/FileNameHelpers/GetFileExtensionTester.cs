using NUnit.Framework;
using Gallery.Core.Extensions;

namespace Gallery.UnitTests.FileNameHelpers
{
    [TestFixture]
    public class GetFileExtensionTester
    {
        [TestCase("file.jpg", "jpg")]
        [TestCase(@"C:\path\fileToCheck.png", "png")]
        [TestCase("C:/path/to/another.gif", "gif")]
        public void ShouldReturnExtensionOfGivenFileName(string fileName, string expectedExtension)
        {
            string fileExtension = fileName.GetFileExtension();

            Assert.AreEqual(fileExtension, expectedExtension, "Incorrect file extension returned.");
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("   ")]
        public void ShouldReturnEmptyStringWhenGivenNullOrEmptyFileName(string fileName)
        {
            string fileExtension = fileName.GetFileExtension();

            Assert.IsEmpty(fileExtension, "Empty file extension expected.");
        }

        [TestCase("TheFile")]
        [TestCase(@"C:\Path\To\File")]
        [TestCase("D:/Parse")]
        public void ShouldReturnEmptyStringWhenGivenFileNameHasNoExtension(string fileName)
        {
            string fileExtension = fileName.GetFileExtension();

            Assert.IsEmpty(fileExtension, "Empty file extension expected.");
        }
    }
}