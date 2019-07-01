using System;
using SQLite;

namespace SQLite_Migrations.Tests
{
    public class Migration201 : BaseMigration
    {
        private string[] scripts = new string[]
        {
            @"CREATE TABLE IF NOT EXISTS [Person]
            (
                [Id]            INTEGER PRIMARY KEY AUTOINCREMENT,
                [Name]          TEXT NOT NULL,
                [Email]         TEXT NOT NULL
            );",
            @"CREATE TABLE IF NOT EXISTS [Stuff]
            (
                [Id]            INTEGER PRIMARY KEY AUTOINCREMENT,
                [PersonId]      INTEGER NOT NULL,
                [Description]   TEXT NOT NULL,
                FOREIGN KEY (PersonId) REFERENCES Person(Id)
            );"
        };

        protected override void OnMigrate(SQLiteConnection db)
        {
            foreach (var script in scripts)
            {
                Migrate(db, script);
            }
        }
    }
}
