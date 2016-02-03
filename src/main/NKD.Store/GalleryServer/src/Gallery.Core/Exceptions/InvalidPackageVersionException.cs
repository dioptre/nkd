using System;

namespace Gallery.Core.Exceptions
{
    public class InvalidPackageVersionException: Exception
    {
        public InvalidPackageVersionException(string packageVersion)
            : base(string.Format("The PackageVersion '{0}' is not valid.", packageVersion))
        { }
    }
}