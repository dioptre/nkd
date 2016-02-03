using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;
using Gallery.Infrastructure.Repositories;

namespace Gallery.UnitTests.TestHelpers
{
    public class FakeStringRepository : RepositoryBase<string>
    {
        public readonly FakeStringDbSet FakeSet;

        public FakeStringRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        {
            FakeSet = new FakeStringDbSet();
        }

        protected override IDbSet<string> DbSet { get { return FakeSet; } }

        protected override Expression<Func<string, bool>> GetByIdExpression(string obj)
        {
            throw new NotImplementedException();
        }
    }
}