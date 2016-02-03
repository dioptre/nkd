using System;

namespace Gallery.Core.Exceptions
{
    public class PackageTooBigException : Exception
    {
        public PackageTooBigException() : base("The package exceeded the maximum size allowed by the gallery.")
        {
        }
    }
}