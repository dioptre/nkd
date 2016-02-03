using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations.Tables
{
    [Migration(20110104093730)]
    public class PackageDataAggregate : Migration
    {
        private const string TABLE_NAME = "PackageDataAggregate";

        public override void Up()
        {
            Database.AddTable(TABLE_NAME,
               new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
               new Column("PackageId", DbType.String, 128, ColumnProperty.NotNull),
               new Column("Rating", DbType.Double, 128, ColumnProperty.NotNull),
               new Column("RatingsCount", DbType.Int32, ColumnProperty.NotNull),
               new Column("DownloadCount", DbType.Int32, ColumnProperty.NotNull)
            );

            Database.AddUniqueConstraint("PackageDataAggregate_PackageId", TABLE_NAME, "PackageId");
        }

        public override void Down()
        {
            Database.RemoveTable(TABLE_NAME);
        }
    }
}