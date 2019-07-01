using SQLite;

namespace SQLite_Migrations.Tests
{
    public class Migration101 : BaseMigration
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

        protected override void OnSeed(SQLiteConnection db)
        {
            db.RunInTransaction(() =>
            {
                var john = new Person { Name = "John", Email = "john@home.com" };
                var johnKey = db.Insert(john);
                db.Insert(new Stuff { PersonId = johnKey, Description = "Oranges" });
                db.Insert(new Stuff { PersonId = johnKey, Description = "Apples" });

                var tim = new Person { Name = "Tim", Email = "tim@work.com" };
                var timKey = db.Insert(tim);
                db.Insert(new Stuff { PersonId = timKey, Description = "Pears" });
                db.Insert(new Stuff { PersonId = timKey, Description = "Cherries" });
            });
        }

        public class Person
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public class Stuff
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string Description { get; set; }
        }
    }
}
