using System.Linq;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;

namespace Gallery.Plugins.NuPackPackageFactory
{
    public class ZipPackageMapper : IPackageMapper<IZipPackage>
    {
        private readonly IDependencyMapper _dependencyMapper;

        public ZipPackageMapper()
            : this(new DependencyMapper())
        { }

        public ZipPackageMapper(IDependencyMapper dependencyMapper)
        {
            _dependencyMapper = dependencyMapper;
        }

        public Package Map(IZipPackage zipPackage)
        {
            string packageVersion = zipPackage.Version.ToSafeString();
            string licenseUrl = zipPackage.LicenseUrl.ToSafeString();
            string iconUrl = zipPackage.IconUrl.ToSafeString();
            string projectUrl = zipPackage.ProjectUrl.ToSafeString();
            return new Package
            {
                Id = zipPackage.Id,
                Title = string.IsNullOrWhiteSpace(zipPackage.Title) ? zipPackage.Id : zipPackage.Title,
                Version = packageVersion,
                Authors = string.Join(", ", zipPackage.Authors ?? new string[0]),
                Description = zipPackage.Description,
                Language = zipPackage.Language,
                LicenseUrl = licenseUrl,
                Summary = zipPackage.Summary,
                IconUrl = iconUrl,
                ProjectUrl = projectUrl,
                Dependencies = _dependencyMapper.Map(zipPackage.Dependencies, zipPackage.Id, packageVersion).ToList(),
                RequireLicenseAcceptance = zipPackage.RequireLicenseAcceptance
            };
        }
    }
}