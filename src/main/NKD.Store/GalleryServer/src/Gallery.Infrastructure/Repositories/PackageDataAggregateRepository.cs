using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class PackageDataAggregateRepository : RepositoryBase<PackageDataAggregate>
    {
        public PackageDataAggregateRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<PackageDataAggregate> DbSet { get { return DatabaseContext.PackageDataAggregates; } }

        protected override Expression<Func<PackageDataAggregate, bool>> GetByIdExpression(PackageDataAggregate packageDataAggregate)
        {
            return pda => pda.Id == packageDataAggregate.Id;
        }
    }
}