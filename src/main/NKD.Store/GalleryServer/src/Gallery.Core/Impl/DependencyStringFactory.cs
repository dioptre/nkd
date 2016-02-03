using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using System.Linq;

namespace Gallery.Core.Impl
{
    public class DependencyStringFactory : IDependencyStringFactory
    {
        public string CreateDependencyString(Dependency dependency)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }
            return string.Format("{0}:{1}", dependency.Name, dependency.VersionSpec);
        }

        public string CreateDependencyListAsString(IEnumerable<Dependency> dependencies)
        {
            return dependencies != null ? string.Join("|", dependencies.Select(CreateDependencyString)) : string.Empty;
        }
    }
}