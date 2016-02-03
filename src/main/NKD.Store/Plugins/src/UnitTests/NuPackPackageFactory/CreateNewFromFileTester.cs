using System.IO;
using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;
using IZipPackage = Gallery.Plugins.NuPackPackageFactory.IZipPackage;

namespace Gallery.Plugins.UnitTests.NuPackPackageFactory
{
    [TestFixture]
    public class CreateNewFromFileTester
    {
        private IPackageFactory _nuPackFactory;
        private Mock<IPackageMapper<IZipPackage>> _mockedZipPackage;
        private Mock<IFileSystem> _mockedFileSystem;
        private Mock<IDateTime> _mockedDateTime;

        [SetUp]
        public void SetUp()
        {
            _mockedZipPackage = new Mock<IPackageMapper<IZipPackage>>();
            _mockedFileSystem = new Mock<IFileSystem>();
            _mockedDateTime = new Mock<IDateTime>();
            _nuPackFactory = new Plugins.NuPackPackageFactory.NuPackPackageFactory(_mockedFileSystem.Object, _mockedZipPackage.Object, _mockedDateTime.Object);
        }

        [Test]
        public void ShouldThrowWhenFileSystemThrows()
        {
            const string pathToPackageFile = "path to file.png";
            _mockedFileSystem.Setup(fs => fs.OpenRead(pathToPackageFile)).Throws(new FileNotFoundException());

            TestDelegate methodThatShouldThrow = () => _nuPackFactory.CreateNewFromFile(pathToPackageFile);

            Assert.Throws<FileNotFoundException>(methodThatShouldThrow, "CreateNew should throw when given file is not found.");
        }
    }
}