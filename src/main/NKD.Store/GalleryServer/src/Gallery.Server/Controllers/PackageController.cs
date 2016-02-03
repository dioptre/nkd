using System.IO;
using System.Web.Mvc;
using Gallery.Core;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using System.Linq;

namespace Gallery.Server.Controllers
{
    public class PackageController : Controller
    {
        private readonly IPackageDownloadCountIncrementer _packageDownloadCountIncrementer;
        private readonly IPackageFileGetter _packageFileGetter;
        private readonly IRepository<PublishedPackage> _publishedPackageRepository;
        private readonly ITaskScheduler _taskScheduler;

        public PackageController()
            : this(IoC.Resolver.Resolve<IPackageDownloadCountIncrementer>(), IoC.Resolver.Resolve<IPackageFileGetter>(),
            IoC.Resolver.Resolve<IRepository<PublishedPackage>>(), IoC.Resolver.Resolve<ITaskScheduler>())
        { }

        public PackageController(IPackageDownloadCountIncrementer packageDownloadCountIncrementer, IPackageFileGetter packageFileGetter,
            IRepository<PublishedPackage> publishedPackageRepository, ITaskScheduler taskScheduler)
        {
            _packageDownloadCountIncrementer = packageDownloadCountIncrementer;
            _packageFileGetter = packageFileGetter;
            _publishedPackageRepository = publishedPackageRepository;
            _taskScheduler = taskScheduler;
        }

        [HttpGet]
        public ActionResult Download(string packageId, string packageVersion)
        {
            PublishedPackage publishedPackage = _publishedPackageRepository.Collection
                .SingleOrDefault(pp => pp.Id == packageId && pp.Version == packageVersion);

            if (publishedPackage == null)
            {
                throw new PackageFileDoesNotExistException(packageId, packageVersion);
            }

            _taskScheduler.ScheduleTask(() => _packageDownloadCountIncrementer.Increment(publishedPackage.Id, publishedPackage.Version));

            if (!string.IsNullOrWhiteSpace(publishedPackage.ExternalPackageUrl))
            {
                return new RedirectResult(publishedPackage.ExternalPackageUrl);
            }
            Stream packageFile = _packageFileGetter.GetPackageStream(publishedPackage.Id, publishedPackage.Version);
            return File(packageFile, "application/zip", string.Format("{0}-{1}.nupkg", packageId, packageVersion));
        }
    }
}