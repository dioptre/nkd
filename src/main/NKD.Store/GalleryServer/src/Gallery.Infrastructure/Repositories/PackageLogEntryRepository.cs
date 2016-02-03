using System;
using System.Data.Entity;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Repositories
{
    public class PackageLogEntryRepository : RepositoryBase<PackageLogEntry>
    {
        public PackageLogEntryRepository(IDatabaseContext databaseContext, IMapper mapper)
            : base(databaseContext, mapper)
        { }

        protected override IDbSet<PackageLogEntry> DbSet
        {
            get { return DatabaseContext.PackageLogEntries; }
        }

        protected override Expression<Func<PackageLogEntry, bool>> GetByIdExpression(PackageLogEntry packageLogEntry)
        {
            return dp => dp.Id == packageLogEntry.Id;
        }
    }
}