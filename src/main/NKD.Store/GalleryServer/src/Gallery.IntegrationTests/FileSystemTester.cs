using System;
using System.IO;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    public class FileSystemTester : FileSystemTesterBase<IFileSystem>
    {
        private static void AssertDirectoryWasCreated(string directory)
        {
            try
            {
                Assert.IsTrue(Directory.Exists(directory), "Directory was not created.");
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory);
                }
            }
        }

        [Test]
        public void SaveShouldBeAbleToSaveFile()
        {
            string destinationFileName = Guid.NewGuid().ToString();

            using (FileStream existingFileStream = File.OpenRead(EXISTING_FILE_NAME))
            {
                Instance.Save(existingFileStream, destinationFileName);
            }

            if (!File.Exists(destinationFileName))
            {
                Assert.Fail("File was not saved.");
            }
            File.Delete(destinationFileName);
        }

        [Test]
        public void SaveShouldBeAbleToSaveFileInNonexistentDirectory()
        {
            string nonexistentDirectory = Guid.NewGuid().ToString();
            string destinationFileName = Path.Combine(nonexistentDirectory, EXISTING_FILE_NAME);

            using (FileStream existingFileStream = File.OpenRead(EXISTING_FILE_NAME))
            {
                Instance.Save(existingFileStream, destinationFileName);
            }

            if (!File.Exists(destinationFileName))
            {
                Assert.Fail("File could not be saved to nonexistent location.");
            }
            File.Delete(destinationFileName);
            Directory.Delete(nonexistentDirectory);
        }

        [Test]
        public void MoveShouldMoveFileFromOneLocationToAnother()
        {
            string destinationFileName = Guid.NewGuid().ToString();
            Instance.Move(EXISTING_FILE_NAME, destinationFileName, false);
            if (File.Exists(EXISTING_FILE_NAME))
            {
                Assert.Fail("Original file still exists. It should have been moved to the new location.");
            }
            if (!File.Exists(destinationFileName))
            {
                Assert.Fail("The file was not moved to the new location.");
            }
            File.Delete(destinationFileName);
        }

        [Test]
        public void MoveShouldBeAbleToOverwriteExistingDestination()
        {
            string destinationFileName = Guid.NewGuid().ToString();
            using (FileStream existingFileStream = File.OpenRead(EXISTING_FILE_NAME))
            {
                Instance.Save(existingFileStream, destinationFileName);
            }

            Instance.Move(EXISTING_FILE_NAME, destinationFileName, true);

            if (File.Exists(EXISTING_FILE_NAME))
            {
                Assert.Fail("Original file still exists. It should have been moved to the new location.");
            }
            if (!File.Exists(destinationFileName))
            {
                Assert.Fail("The file was not moved to the new location.");
            }
            File.Delete(destinationFileName);
        }

        [Test]
        public void DeleteFileIfItExistsShouldNotBombIfFileDoesNotExist()
        {
            string fileThatDoesNotExist = Guid.NewGuid().ToString();

            Instance.DeleteFileIfItExists(fileThatDoesNotExist);
        }

        [Test]
        public void DeleteFileIfItExistsShouldDeleteExistingFile()
        {
            Instance.DeleteFileIfItExists(EXISTING_FILE_NAME);

            bool fileStillExists = File.Exists(EXISTING_FILE_NAME);

            Assert.IsFalse(fileStillExists, "Existing file was not deleted.");
        }

        [Test]
        public void CreateDirectoryForFileIfNonexistentShouldNotBombWhenDirectoryAlreadyExists()
        {
            Instance.CreateDirectoryForFileIfNonexistent(EXISTING_FILE_NAME);
        }

        [Test]
        public void CreateDirectoryForFileIfNonexistentShouldCreateDirectory()
        {
            string newDirectory = Guid.NewGuid().ToString();
            string newFileName = Path.Combine(newDirectory, EXISTING_FILE_NAME);

            Instance.CreateDirectoryForFileIfNonexistent(newFileName);

            AssertDirectoryWasCreated(newDirectory);
        }

        [Test]
        public void CreateDirectoryIfNonexistentShouldCreateDirectory() {
            string newDirectory = Guid.NewGuid().ToString();

            Instance.CreateDirectoryIfNonexistent(newDirectory);

            AssertDirectoryWasCreated(newDirectory);
        }

        [Test]
        public void CreateDirectoryIfNonexistentShouldNotBombWhenDirectoryAlreadyExists() {
            const string existingDirectory = @"C:\";

            Instance.CreateDirectoryIfNonexistent(existingDirectory);

            Assert.DoesNotThrow(() => Instance.CreateDirectoryIfNonexistent(existingDirectory));
        }

        [Test]
        public void OpenReadShouldOpenExistingFileWithoutThrowing()
        {
            using (Instance.OpenRead(EXISTING_FILE_NAME))
            { }
        }

        [Test]
        public void OpenReadShouldThrowWhenFileDoesNotExist()
        {
            string nonexistentFile = Guid.NewGuid().ToString();

            TestDelegate methodThatShouldThrow = () => Instance.OpenRead(nonexistentFile);

            Assert.Throws<FileNotFoundException>(methodThatShouldThrow, "OpenRead should have thrown an exception for nonexistent file.");
        }
    }
}