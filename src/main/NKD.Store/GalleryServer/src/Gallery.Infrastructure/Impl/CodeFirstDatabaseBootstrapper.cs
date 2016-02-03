using System.Data.Entity;
using Gallery.Core.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class CodeFirstDatabaseBootstrapper : IDatabaseBootstrapper
    {
        public void InitializeDatabase()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<GalleryFeedEntities>());
        }
    }
}