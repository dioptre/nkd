using System.Data.Entity;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Domain;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class PackageGetter : IPackageGetter
    {
        private readonly IPackageAuthenticator _packageAuthenticator;
        private readonly IRepository<Package> _packageRepository;

        public PackageGetter(IPackageAuthenticator packageAuthenticator, IRepository<Package> packageRepository)
        {
            _packageAuthenticator = packageAuthenticator;
            _packageRepository = packageRepository;
        }

        public Package GetPackage(string key, string id, string version)
        {
            _packageAuthenticator.EnsureKeyCanAccessPackage(key, id, version);
            IQueryable<Package> packages = _packageRepository.Collection.Include(p => p.Screenshots).Where(p => p.Id == id && p.Version == version);
            if (!packages.Any())
            {
                throw new PackageDoesNotExistException(id, version);
            }
            if (packages.Count() > 1)
            {
                throw new WebFaultException<string>(string.Format("Duplicate Packages with ID '{0}' and Version '{1}' were found.", id, version),
                    HttpStatusCode.NotFound);
            }
            return packages.Single();
        }
    }
}