using System.Text.RegularExpressions;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class PackageIdValidator : IPackageIdValidator
    {
        public bool IsValidPackageId(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return false;
            }
            Regex regex = new Regex(@"^[a-z0-9A-Z]+([_.-][a-z0-9A-Z]+)*$");
            return regex.IsMatch(packageId);
        }
    }
}