using System;
using System.IO;
using System.Linq;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class WindowsFileSystem : IFileSystem
    {
        public void Save(Stream stream, string destinationFileName)
        {
            CreateDirectoryForFileIfNonexistent(destinationFileName);
            using (var writeStream = File.OpenWrite(destinationFileName))
            {
                const int length = 256;
                var buffer = new Byte[length];
                int bytesRead = stream.Read(buffer, 0, length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = stream.Read(buffer, 0, length);
                }
            }
        }

        public void Move(string sourceFileName, string destinationFileName, bool overwriteIfDestinationExists)
        {
            if (overwriteIfDestinationExists)
            {
                DeleteFileIfItExists(destinationFileName);
            }
            CreateDirectoryForFileIfNonexistent(destinationFileName);
            File.Move(sourceFileName, destinationFileName);
        }

        public void DeleteFileIfItExists(string fileName)
        {
            if (FileExists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public void DeleteDirectoryIfItExists(string directoryName, bool recursive)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, recursive);
            }
        }

        public void DeleteDirectoryIfEmpty(string directoryName, bool recursive)
        {
            if (Directory.Exists(directoryName) && Directory.EnumerateFiles(directoryName).Count() == 0)
            {
                Directory.Delete(directoryName, recursive);
            }
        }

        public void CreateDirectoryForFileIfNonexistent(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);

        }

        public void CreateDirectoryIfNonexistent(string directoryName)
        {
            if (!Directory.Exists(directoryName)) {
                Directory.CreateDirectory(directoryName);
            }
        }

        public FileStream OpenRead(string path)
        {
            return File.OpenRead(path);
        }
    }
}