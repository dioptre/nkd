using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations.Tables
{
    [Migration(20110102144842)]
    public class Dependency : Migration
    {
        private const string TableName = "Dependency";

        public override void Up()
        {
            Database.AddTable(TableName,
               new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
               new Column("PackageId", DbType.String, 128, ColumnProperty.NotNull),
               new Column("PackageVersion", DbType.String, 128, ColumnProperty.NotNull),
               new Column("Name", DbType.String, 4000, ColumnProperty.Null),
               new Column("VersionSpec", DbType.String, 4000, ColumnProperty.Null)
            );

            Database.AddForeignKey("Package_Dependencies",
                TableName, new[] { "PackageId", "PackageVersion" },
                "Package", new[] { "Id", "Version" });
        }

        public override void Down()
        {
            Database.RemoveTable(TableName);
        }
    }
}
