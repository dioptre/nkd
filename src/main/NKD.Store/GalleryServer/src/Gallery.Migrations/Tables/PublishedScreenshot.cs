using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations.Tables
{
    [Migration(20110102145910)]
    public class PublishedScreenshot : Migration
    {
        private const string TableName = "PublishedScreenshot";

        public override void Up()
        {
            Database.AddTable(TableName,
               new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
               new Column("PublishedPackageId", DbType.String, 128, ColumnProperty.NotNull),
               new Column("PublishedPackageVersion", DbType.String, 128, ColumnProperty.NotNull),
               new Column("ScreenshotUri", DbType.String, 4000, ColumnProperty.Null),
               new Column("Caption", DbType.String, 4000, ColumnProperty.Null)
            );

            Database.AddForeignKey("PublishedPackage_Screenshots",
                TableName, new[] { "PublishedPackageId", "PublishedPackageVersion" },
                "PublishedPackage", new[] { "Id", "Version" });
        }

        public override void Down()
        {
            Database.RemoveTable(TableName);
        }
    }
}
