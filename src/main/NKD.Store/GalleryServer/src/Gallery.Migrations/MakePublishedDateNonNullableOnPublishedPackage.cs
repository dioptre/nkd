using Migrator.Framework;

namespace Gallery.Migrations
{
    [Migration(20110409085930)]
    public class MakePublishedDateNonNullableOnPublishedPackage : Migration
    {
        private const string PUBLISHED_PACKAGE_TABLE_NAME = "PublishedPackage";
        private const string PUBLISHED_DATE_COLUMN_NAME = "Published";

        public override void Up()
        {
            Database.ExecuteNonQuery(string.Format("ALTER TABLE {0} ALTER COLUMN {1} DateTime NOT NULL", PUBLISHED_PACKAGE_TABLE_NAME, PUBLISHED_DATE_COLUMN_NAME));
        }

        public override void Down()
        {
            Database.ExecuteNonQuery(string.Format("ALTER TABLE {0} ALTER COLUMN {1} DateTime NULL", PUBLISHED_PACKAGE_TABLE_NAME,
                PUBLISHED_DATE_COLUMN_NAME));
        }
    }
}