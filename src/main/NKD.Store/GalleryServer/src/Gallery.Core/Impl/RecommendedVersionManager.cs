using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class RecommendedVersionManager<T> : IRecommendedVersionManager<T>
        where T : class, IVersionable
    {
        private readonly ILatestVersionGetter<T> _latestVersionGetter;
        private readonly IRecommendedVersionSetter<T> _recommendedVersionSetter;

        public RecommendedVersionManager(ILatestVersionGetter<T> latestVersionGetter, IRecommendedVersionSetter<T> recommendedVersionSetter)
        {
            _latestVersionGetter = latestVersionGetter;
            _recommendedVersionSetter = recommendedVersionSetter;
        }

        public void SetLatestVersionAsRecommended(string packageId, bool shouldLogPackageAction)
        {
            var recommendedPackageVersion = _latestVersionGetter.GetLatestVersion(packageId);
            _recommendedVersionSetter.SetAsRecommendedVersion(recommendedPackageVersion, shouldLogPackageAction);
        }
    }
}