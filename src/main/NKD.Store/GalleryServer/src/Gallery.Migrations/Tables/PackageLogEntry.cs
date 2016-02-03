using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations.Tables
{
    [Migration(20110102150407)]
    public class PackageLogEntry : Migration
    {
        private const string TableName = "PackageLogEntry";

        public override void Up()
        {
            Database.AddTable(TableName,
               new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
               new Column("PackageId", DbType.String, 128, ColumnProperty.NotNull),
               new Column("PackageVersion", DbType.String, 128, ColumnProperty.NotNull),
               new Column("DateLogged", DbType.DateTime, ColumnProperty.NotNull),
               new Column("ActionValue", DbType.Int32, ColumnProperty.NotNull)
            );
        }

        public override void Down()
        {
            Database.RemoveTable(TableName);
        }
    }
}
