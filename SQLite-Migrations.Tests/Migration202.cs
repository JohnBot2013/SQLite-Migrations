using SQLite;

namespace SQLite_Migrations.Tests
{
    public class Migration202 : BaseMigration
    {
        protected override void OnMigrate(SQLiteConnection db)
        {
            if (!ColumnExists(db, "Person", "Age"))
            {
                var sql = "ALTER TABLE Person ADD COLUMN Age INTEGER NULL";
                Migrate(db, sql);
            }
        }
    }
}
