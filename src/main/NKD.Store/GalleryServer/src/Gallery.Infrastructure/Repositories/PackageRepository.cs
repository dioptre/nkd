using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class PackageRepository : RepositoryBase<Package>
    {
        public PackageRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<Package> DbSet { get { return DatabaseContext.Packages; } }

        protected override Expression<Func<Package, bool>> GetByIdExpression(Package package)
        {
            return p => p.Id == package.Id && p.Version == package.Version;
        }

        protected override Package GetById(Package obj)
        {
            return DbSet.Include(p => p.Screenshots).Single(GetByIdExpression(obj));
        }
    }
}