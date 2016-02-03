using System;

namespace Gallery.Core.Exceptions
{
    public class InvalidPackageIdException : Exception
    {
        public InvalidPackageIdException(string packageId)
            : base(string.Format("The PackageId '{0}' is not valid.", packageId))
        { }
    }
}