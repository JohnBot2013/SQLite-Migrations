using System;
using System.Linq;

using SQLite;

namespace SQLite_Migrations
{
    public abstract class BaseMigration : IMigration
    {
        private readonly Type type;

        protected BaseMigration()
        {
            this.type = this.GetType();
        }

        public int Version => GetVersionNumber();

        public void PreMigrate(SQLiteConnection db)
        {
            OnPreMigrate(db);
        }

        protected virtual void OnPreMigrate(SQLiteConnection db)
        {
            // NOP
        }

        public void Migrate(SQLiteConnection db)
        {
            OnMigrate(db);
        }

        protected virtual void OnMigrate(SQLiteConnection db)
        {
            // NOP
        }

        public void PostMigrate(SQLiteConnection db)
        {
            OnPostMigrate(db);
        }

        protected virtual void OnPostMigrate(SQLiteConnection db)
        {
            // NOP
        }

        public void Seed(SQLiteConnection db)
        {
            OnSeed(db);
        }

        protected virtual void OnSeed(SQLiteConnection db)
        {
            // NOP
        }

        protected void Migrate(SQLiteConnection db, string sql)
        {
            UpdateDb(db, sql);
            UpdateVersion(db);
        }

        protected void UpdateVersion(SQLiteConnection db)
        {
            UpdateDb(db, $"PRAGMA user_version = {Version}");
        }

        protected void UpdateDb(SQLiteConnection db, string sql)
        {
            db.Execute(sql);
        }

        protected bool ColumnExists(SQLiteConnection db, string tableName, string columnName)
        {
            var list = db.Query<ColumnInfo>($"PRAGMA table_info({tableName})");
            return list.Any(c => c.name == columnName);
        }

        protected int GetVersionNumber()
        {
            string versionString = this.type.Name.Substring(type.Name.Length - 3, 3);
            int versionNumber = 0;
            if (!int.TryParse(versionString, out versionNumber))
                throw new DbInitialiserException($"Migration {this.type.Name} does not have a valid version number as the last 3 characters");
            return versionNumber;
        }

        #region IDisposable Support

        protected virtual void OnDisposing()
        {
            // NOP
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnDisposing();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
