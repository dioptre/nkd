using System.IO;

namespace Gallery.Core.Extensions
{
    public static class FileNameHelpers
    {
        public static string GetFileName(this string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return string.Empty;
            }
            return Path.GetFileName(fullPath);
        }

        public static string GetFileExtension(this string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return extension != null ? extension.TrimStart('.') : string.Empty;
        }
    }
}