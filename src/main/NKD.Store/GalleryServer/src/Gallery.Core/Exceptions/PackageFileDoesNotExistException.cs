using System;

namespace Gallery.Core.Exceptions
{
    public class PackageFileDoesNotExistException : Exception
    {
        public PackageFileDoesNotExistException()
            : base("Package does not have a corresponding file.")
        { }

        public PackageFileDoesNotExistException(string packageId, string packageVersion, Exception innerException = null)
            : base(string.Format("Package with ID '{0}' and Version '{1}' does not have a corresponding file.", packageId, packageVersion), innerException)
        { }
    }
}