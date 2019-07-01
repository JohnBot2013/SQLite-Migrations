using System;

using SQLite;

namespace SQLite_Migrations
{
    /// <summary>
    /// This interface is used to implement database schema updates on version releases
    /// DbInitialiser implementation uses these migrations to build or update the
    /// database at startup.
    /// Derived classes MUST have 3 digits at the end of their name (i.e. Migration001, Migration002 etc.)
    /// The 3 digits are used to a) determine which Migrations should be run and b) the order in which to run them.
    /// </summary>
    public interface IMigration : IDisposable
    {
        /// <summary>
        /// Provides the version number of this set of database changes
        /// </summary>
        int Version { get; }
        /// <summary>
        /// Perform any actions on the database before changing the schema. This might 
        /// involve copying data to temp tables etc to avoid data loss
        /// </summary>
        /// <param name="db"></param>
        void PreMigrate(SQLiteConnection db);
        /// <summary>
        /// Update the database schema
        /// The final task must be to update the version number in Sqlite
        /// </summary>
        /// <param name="db"></param>
        void Migrate(SQLiteConnection db);
        /// <summary>
        /// Perform any actions on the database after changing the schema. This might
        /// include copying data back from temporary tables and then cleaning up.
        /// </summary>
        /// <param name="db"></param>
        void PostMigrate(SQLiteConnection db);
        /// <summary>
        /// Perform any seeding needed by the database. This might include setting
        /// new column values to a default as well as genuine data seeding
        /// </summary>
        /// <param name="db"></param>
        void Seed(SQLiteConnection db);
    }
}
