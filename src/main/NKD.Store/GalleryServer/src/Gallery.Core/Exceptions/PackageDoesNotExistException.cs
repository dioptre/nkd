using Gallery.Core.Domain;

namespace Gallery.Core.Exceptions
{
    public class PackageDoesNotExistException : ObjectDoesNotExistException<Package>
    {
        public PackageDoesNotExistException(string packageId, string packageVersion)
            : base(string.Format("Package with ID '{0}' and Version '{1}' does not exist.", packageId, packageVersion))
        { }
    }
}