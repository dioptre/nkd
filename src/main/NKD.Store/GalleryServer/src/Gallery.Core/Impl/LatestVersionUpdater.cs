using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class LatestVersionUpdater<TVersionable> : ILatestVersionUpdater<TVersionable>
        where TVersionable : class, IVersionable
    {
        private readonly IRepository<TVersionable> _versionableRepository;

        public LatestVersionUpdater(IRepository<TVersionable> versionableRepository)
        {
            _versionableRepository = versionableRepository;
        }

        public void SetLatestVersionFlagsOfOtherVersionablesWithSameId(TVersionable versionable)
        {
            if (!versionable.IsLatestVersion)
            {
                return;
            }
            IList<TVersionable> versionablesToUpdate = _versionableRepository.Collection
                .Where(p => p.Id == versionable.Id && p.Version != versionable.Version && p.IsLatestVersion).ToList();
            foreach (TVersionable versionableToUpdate in versionablesToUpdate)
            {
                versionableToUpdate.IsLatestVersion = false;
            }
            _versionableRepository.Update(versionablesToUpdate);
        }
    }
}