using System;
using System.IO;
using NUnit.Framework;

namespace Gallery.IntegrationTests
{
    public abstract class FileSystemTesterBase<T> : IntegrationTesterBase<T>
    {
        protected const string EXISTING_FILE_NAME = "integration_test.txt";

        [SetUp]
        public void SetUp()
        {
            if (File.Exists(EXISTING_FILE_NAME))
            {
                File.Delete(EXISTING_FILE_NAME);
            }
            using (StreamWriter streamWriter = File.CreateText(EXISTING_FILE_NAME))
            {
                streamWriter.Write("Some text for the integration test file.{0}", Environment.NewLine);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(EXISTING_FILE_NAME))
            {
                File.Delete(EXISTING_FILE_NAME);
            }
        }
    }
}