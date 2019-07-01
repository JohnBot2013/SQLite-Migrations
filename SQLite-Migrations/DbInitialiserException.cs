using System;

namespace SQLite_Migrations
{
    /// <summary>
    /// DbMigrations error
    /// </summary>
    public class DbInitialiserException : Exception
    {
        public DbInitialiserException(string message) : base(message)
        {
        }

        public DbInitialiserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
