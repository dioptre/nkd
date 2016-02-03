using System.Data;
using Migrator.Framework;

namespace Gallery.Migrations
{
    [Migration(20110106100922)]
    public class AddAggregateFieldsToPublishedPackage : Migration
    {
        private const string TABLE_NAME = "PublishedPackage";
        private const string DOWNLOAD_COUNT_COLUMN_NAME = "DownloadCount";
        private const string RATINGS_COUNT_COLUMN_NAME = "RatingsCount";
        private const string RATING_COLUMN_NAME = "Rating";

        public override void Up()
        {
            Database.AddColumn(TABLE_NAME, RATING_COLUMN_NAME, DbType.Double, 0, ColumnProperty.NotNull, 0);
            Database.AddColumn(TABLE_NAME, RATINGS_COUNT_COLUMN_NAME, DbType.Int32, 0, ColumnProperty.NotNull, 0);
            Database.AddColumn(TABLE_NAME, DOWNLOAD_COUNT_COLUMN_NAME, DbType.Int32, 0, ColumnProperty.NotNull, 0);
        }

        public override void Down()
        {
            Database.RemoveColumn(TABLE_NAME, DOWNLOAD_COUNT_COLUMN_NAME);
            Database.RemoveColumn(TABLE_NAME, RATINGS_COUNT_COLUMN_NAME);
            Database.RemoveColumn(TABLE_NAME, RATING_COLUMN_NAME);
        }
    }
}