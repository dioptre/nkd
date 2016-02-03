using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations.Tables
{
    [Migration(20110102145630)]
    public class PublishedPackage : Migration
    {
        private const string TABLE_NAME = "PublishedPackage";

        public override void Up()
        {
            Database.AddTable(TABLE_NAME,
               new Column("Id", DbType.String, 128, ColumnProperty.PrimaryKeyWithIdentity),
               new Column("Version", DbType.String, 128, ColumnProperty.PrimaryKey),
               new Column("Title", DbType.String, 4000, ColumnProperty.Null),
               new Column("Authors", DbType.String, 4000, ColumnProperty.Null),
               new Column("PackageType", DbType.String, 4000, ColumnProperty.Null),
               new Column("Summary", DbType.String, 4000, ColumnProperty.Null),
               new Column("Description", DbType.String, 4000, ColumnProperty.Null),
               new Column("Copyright", DbType.String, 4000, ColumnProperty.Null),
               new Column("PackageHashAlgorithm", DbType.String, 4000, ColumnProperty.Null),
               new Column("PackageHash", DbType.String, 4000, ColumnProperty.Null),
               new Column("PackageSize", DbType.Int64, ColumnProperty.NotNull),
               new Column("Price", DbType.Decimal, ColumnProperty.NotNull),
               new Column("RequireLicenseAcceptance", DbType.Boolean, ColumnProperty.NotNull),
               new Column("IsLatestVersion", DbType.Boolean, ColumnProperty.NotNull),
               new Column("VersionRating", DbType.Double, ColumnProperty.NotNull),
               new Column("VersionRatingsCount", DbType.Int32, ColumnProperty.NotNull),
               new Column("VersionDownloadCount", DbType.Int32, ColumnProperty.NotNull),
               new Column("Created", DbType.DateTime, ColumnProperty.NotNull),
               new Column("LastUpdated", DbType.DateTime, ColumnProperty.NotNull),
               new Column("Published", DbType.DateTime, ColumnProperty.NotNull),
               new Column("ExternalPackageUrl", DbType.String, 4000, ColumnProperty.Null),
               new Column("ProjectUrl", DbType.String, 4000, ColumnProperty.Null),
               new Column("LicenseUrl", DbType.String, 4000, ColumnProperty.Null),
               new Column("IconUrl", DbType.String, 4000, ColumnProperty.Null),
               new Column("Categories", DbType.String, 4000, ColumnProperty.Null),
               new Column("Tags", DbType.String, 4000, ColumnProperty.Null),
               new Column("Dependencies", DbType.String, 4000, ColumnProperty.Null)
            );
        }

        public override void Down()
        {
            Database.RemoveTable(TABLE_NAME);
        }
    }
}
