using System;

namespace Gallery.Core.Exceptions
{
    public class PackageAuthorizationException : Exception
    {
        public PackageAuthorizationException()
            : base("Access denied for Package(s).")
        { }

        public PackageAuthorizationException(string packageId)
            : base(string.Format("Access denied for Package with ID '{0}'.", packageId))
        { }
    }
}