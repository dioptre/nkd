using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Contants;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class LatestVersionGetter<T> : ILatestVersionGetter<T>
        where T : class, IVersionable
    {
        private readonly IRepository<T> _repository;

        public LatestVersionGetter(IRepository<T> repository)
        {
            _repository = repository;
        }

        public T GetLatestVersion(string packageId)
        {
            if (String.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentNullException("packageId");
            }

            IList<T> existingVersions = _repository.Collection.Where(p => p.Id == packageId && p.Published != Constants.UnpublishedDate).ToList();
            return existingVersions.Any()
                ? existingVersions.Select(p => new {Package = p, Version = new Version(p.Version)})
                    .OrderByDescending(v => v.Version).First().Package
                : default(T);
        }
    }
}