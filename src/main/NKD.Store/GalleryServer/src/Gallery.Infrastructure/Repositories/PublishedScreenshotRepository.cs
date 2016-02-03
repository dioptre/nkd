using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.FeedModels;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class PublishedScreenshotRepository : RepositoryBase<PublishedScreenshot>
    {
        public PublishedScreenshotRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<PublishedScreenshot> DbSet { get { return DatabaseContext.PublishedScreenshots; } }

        protected override Expression<Func<PublishedScreenshot, bool>> GetByIdExpression(PublishedScreenshot publishedScreenshot)
        {
            return s => s.Id == publishedScreenshot.Id;
        }
    }
}