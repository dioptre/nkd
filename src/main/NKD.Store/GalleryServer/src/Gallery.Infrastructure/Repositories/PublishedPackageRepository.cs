using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class PublishedPackageRepository : RepositoryBase<PublishedPackage>
    {
        public PublishedPackageRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<PublishedPackage> DbSet { get { return DatabaseContext.PublishedPackages; } }
        protected override Expression<Func<PublishedPackage, bool>> GetByIdExpression(PublishedPackage publishedPackage)
        {
            return pp => pp.Id == publishedPackage.Id && pp.Version == publishedPackage.Version;
        }
    }
}