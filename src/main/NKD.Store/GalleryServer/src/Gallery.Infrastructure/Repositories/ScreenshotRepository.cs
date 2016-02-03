using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class ScreenshotRepository : RepositoryBase<Screenshot>
    {
        public ScreenshotRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<Screenshot> DbSet { get { return DatabaseContext.Screenshots; } }
        protected override Expression<Func<Screenshot, bool>> GetByIdExpression(Screenshot screenshot)
        {
            return s => s.Id == screenshot.Id;
        }
    }
}