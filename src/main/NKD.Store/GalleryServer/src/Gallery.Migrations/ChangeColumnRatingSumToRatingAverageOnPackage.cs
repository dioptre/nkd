using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations
{
    [Migration(20110303115000)]
    public class ChangeColumnRatingSumToRatingAverageOnPackage : Migration
    {
        private const string PACKAGE_TABLE_NAME = "Package";
        private const string RATING_SUM_COLUMN_NAME = "RatingSum";
        private const string RATING_AVERAGE_COLUMN_NAME = "RatingAverage";

        public override void Up()
        {
            Database.AddColumn(PACKAGE_TABLE_NAME, RATING_AVERAGE_COLUMN_NAME, DbType.Double, 0, ColumnProperty.NotNull, 0);
            Database.RemoveColumn(PACKAGE_TABLE_NAME, RATING_SUM_COLUMN_NAME);

            if (Database.TableExists("EdmMetadata"))
            {
                Database.ExecuteNonQuery("DELETE FROM EdmMetadata");
            }
        }

        public override void Down()
        {
            Database.AddColumn(PACKAGE_TABLE_NAME, RATING_SUM_COLUMN_NAME, DbType.Int32, 0, ColumnProperty.NotNull, 0);
            Database.RemoveColumn(PACKAGE_TABLE_NAME, RATING_AVERAGE_COLUMN_NAME);
        }
    }
}