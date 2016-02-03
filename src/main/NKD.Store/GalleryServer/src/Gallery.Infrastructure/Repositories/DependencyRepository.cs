using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class DependencyRepository : RepositoryBase<Dependency>
    {
        public DependencyRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<Dependency> DbSet { get { return DatabaseContext.Dependencies; } }

        protected override Expression<Func<Dependency, bool>> GetByIdExpression(Dependency dependency)
        {
            return p => p.PackageId == dependency.PackageId && p.PackageVersion == dependency.PackageVersion;
        }
    }
}