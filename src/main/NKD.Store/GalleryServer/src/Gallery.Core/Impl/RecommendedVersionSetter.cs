using System.Linq;
using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class RecommendedVersionSetter<T> : IRecommendedVersionSetter<T>
        where T : class, IVersionable
    {
        private readonly IRepository<T> _versionableRepository;
        private readonly IPackageLogEntryCreator _packageLogEntryCreator;

        public RecommendedVersionSetter(IRepository<T> versionableRepository,
            IPackageLogEntryCreator packageLogEntryCreator)
        {
            _versionableRepository = versionableRepository;
            _packageLogEntryCreator = packageLogEntryCreator;
        }

        public void SetAsRecommendedVersion(T versionable, bool shouldLogPackageAction)
        {
            if (versionable == null)
            {
                return;
            }

            versionable.IsLatestVersion = true;
            UpdateVersionable(versionable, shouldLogPackageAction);
            SetOtherVersionsAsNotRecommended(versionable, shouldLogPackageAction);
        }

        private void SetOtherVersionsAsNotRecommended(T versionable, bool shouldLogPackageAction)
        {
            var otherVersionables = _versionableRepository.Collection
                .Where(v => v.Id == versionable.Id && v.Version != versionable.Version && v.IsLatestVersion).ToList();

            foreach (var otherVersionable in otherVersionables)
            {
                otherVersionable.IsLatestVersion = false;
                UpdateVersionable(otherVersionable, shouldLogPackageAction);
            }
        }

        private void UpdateVersionable(T otherVersionable, bool shouldLogPackageAction)
        {
            _versionableRepository.Update(otherVersionable);
            if (shouldLogPackageAction)
            {
                _packageLogEntryCreator.Create(otherVersionable.Id, otherVersionable.Version, PackageLogAction.Update);
            }
        }
    }
}