using System.IO;

namespace Gallery.Core.Interfaces
{
    public interface IFileSystem
    {
        void Save(Stream stream, string destinationFileName);
        void Move(string sourceFileName, string destinationFileName, bool overwriteIfDestinationExists);
        void DeleteFileIfItExists(string fileName);
        void DeleteDirectoryIfItExists(string directoryName, bool recursive = false);
        void DeleteDirectoryIfEmpty(string directoryName, bool recursive = false);
        FileStream OpenRead(string path);
        void CreateDirectoryForFileIfNonexistent(string fileName);
        bool FileExists(string fileName);
        void CreateDirectoryIfNonexistent(string directoryName);
    }
}