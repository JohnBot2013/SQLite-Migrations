using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using SQLite;

namespace SQLite_Migrations
{
    /// <summary>
    /// Perform Migrations on a database by executing a series of IMigration objects.
    /// IMigration classes must be named with trailing 3 digits (i.e. Migration001, Migration002 etc.)
    /// There must be no gaps in the sequence. The name can reflect the purpose of the migration but MUST end with 3 digits.
    /// The 3 digits are used to a) determine which Migrations should be run and b) the order in which to run them.
    /// For example, if you have IMigrations A001, A002, A003 and the database is already at version 2 then only A003
    /// would be executed
    /// </summary>
    public class DbInitialiser
    {
        public event EventHandler<ProgressEventArgs> ProgressEvent;

        private readonly SQLiteConnection connection;
        private readonly Assembly assemblyWithMigrations;

        /// <summary>
        /// Instantiate the Migration controller
        /// </summary>
        /// <param name="connection">A valid SQLite connection</param>
        /// <param name="assemblyWithMigrations">
        /// The assembly containing the IMigrations
        /// If null then the IMigrations are assumed to be in the current assembly.
        /// </param>
        public DbInitialiser(SQLiteConnection connection, Assembly assemblyWithMigrations = null)
        {
            this.connection = connection;
            this.assemblyWithMigrations = assemblyWithMigrations;
        }

        public long InitialVersion { get; private set; }

        public long FinalVersion { get; private set; }

        /// <summary>
        /// Automatically initialise the database. The Initialiser will collect all IMigrations and execute them
        /// in order and as appropriate. These must be correctly named.
        /// </summary>
        public async Task InitialiseDatabase()
        {
            await StartInitialisation();
            var migrations = await GetMigrations(assemblyWithMigrations);
            int increment = 100 / migrations.Count;
            int index = 0;
            foreach (var migration in migrations)
            {
                long configVersion = GetConfigVersion(migration);
                if (configVersion > InitialVersion || InitialVersion == 0)
                    await ExecuteMigration(migration);
                index += increment;
                OnProgress(index, $"Migration {configVersion}");
            }
            await EndInitialisation();
        }

        /// <summary>
        /// Initialise the database with a pre-prepared list of IMigrations.
        /// These must be in the correct order and correctly named
        /// </summary>
        /// <param name="migrations"></param>
        /// <param name="dispose"></param>
        public async Task InitialiseDatabase(IMigration[] migrations, bool dispose = false)
        {
            await StartInitialisation();
            if (migrations == null || migrations.Length == 0)
                throw new ArgumentException("A valid array of IMigration must be provided", nameof(migrations));
            int version = 0;
            int increment = 100 / migrations.Length;
            int index = 0;
            foreach (var migration in migrations)
            {
                if (migration.Version < version)
                    throw new DbInitialiserException($"{migration.GetType().Name}.Version is not in sequence");
                await RunMigration(migration);
                if (dispose)
                    migration.Dispose();
                index += increment;
                OnProgress(index, $"Migration {migration.Version}");
            }
            await EndInitialisation();
        }

        /// <summary>
        /// Verify that the database is clean (no tables, no data)
        /// </summary>
        public bool IsBlankDatabase()
        {
            var tables = connection.Query<TableInfo>("SELECT * FROM sqlite_master where type='table'");
            if (tables == null || tables.Count == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Return a list of table names
        /// </summary>
        public string[] GetTableNames()
        {
            string tableSQL = "SELECT name FROM sqlite_master WHERE type='table'";
            List<string> tables = new List<string>();
            var statement = SQLite3.Prepare2(connection.Handle, tableSQL);
            try
            {
                bool done = false;
                while (!done)
                {
                    SQLite3.Result result = SQLite3.Step(statement);
                    if (result == SQLite3.Result.Row)
                    {
                        var tableName = SQLite3.ColumnString(statement, 0);
                        tables.Add(tableName);
                    }
                    else if (result == SQLite3.Result.Done)
                    {
                        done = true;
                    }
                    else
                    {
                        throw SQLiteException.New(result, SQLite3.GetErrmsg(connection.Handle));
                    }
                }
            }
            finally
            {
                SQLite3.Finalize(statement);
            }
            return tables.ToArray();
        }

        /// <summary>
        /// Get a list of column names for a table
        /// </summary>
        /// <param name="tableName">The name of the table to be examined</param>
        public string[] GetColumnNames(string tableName)
        {
            var list = connection.Query<ColumnInfo>($"PRAGMA table_info({tableName})");
            return list.Select(s => s.name).ToArray();
        }

        private async Task StartInitialisation()
        {
            InitialVersion = await GetCurrentVersion();
            OnProgress(0, "Checking database...");
        }

        private async Task EndInitialisation()
        {
            FinalVersion = await GetCurrentVersion();
            OnProgress(100, "Database check complete...");
        }

        private async Task<bool> ExecuteMigration(Type type)
        {
            if (Activator.CreateInstance(type) is IMigration migration)
            {
                try
                {
                    await RunMigration(migration);
                }
                finally
                {
                    migration.Dispose();
                }
            }
            else
            {
                throw new DbInitialiserException($"Type {type.Name} could not be instantiated as an IMigration");
            }
            return true;
        }

        private Task<bool> RunMigration(IMigration migration)
        {
            try
            {
                connection.RunInTransaction(() =>
                {
                    migration.PreMigrate(connection);
                    migration.Migrate(connection);
                    migration.PostMigrate(connection);
                    migration.Seed(connection);
                });
            }
            catch (Exception exc)
            {
                string migrationName = migration.GetType().Name;
                throw new DbInitialiserException($"Migration {migrationName} could not be run. See inner exception for details", exc);
            }
            return Task.FromResult<bool>(true);
        }

        private async Task<IList<Type>> GetMigrations(Assembly asm)
        {
            var migrations = new List<Type>();
            await Task.Run(() =>
            {
                if (asm == null)
                {
                    // Assume IMigrations are in the current assembly
                    asm = this.GetType().Assembly;
                }

                IEnumerable<Type> types = null;
                try
                {
                    types = asm.GetTypes().Where(t => t.IsPublic && !t.IsInterface && !t.IsAbstract && t.GetInterface("IMigration") != null);
                }
                catch (ReflectionTypeLoadException exc)
                {
                    var loadedTypes = exc.Types.Where(t => t != null);
                    types = loadedTypes.Where(t => t.IsPublic && !t.IsInterface && !t.IsAbstract && t.GetInterface("IMigration") != null);
                }
                foreach (var type in types)
                    migrations.Add(type);
            }).ConfigureAwait(false);

            return migrations;
        }

        private Task<long> GetCurrentVersion()
        {
            long version = -1;
            version = connection.ExecuteScalar<long>("PRAGMA user_version");
            return Task.FromResult<long>(version);
        }

        private long GetConfigVersion(Type type)
        {
            string versionString = type.Name.Substring(type.Name.Length - 3, 3);
            if (!long.TryParse(versionString, out var versionNumber))
                throw new DbInitialiserException($"Migration {type.Name} does not have a valid version number as the last 3 characters");
            return versionNumber;
        }

        private void OnProgress(int value, string message)
        {
            ProgressEvent?.Invoke(this, new ProgressEventArgs(value, message));
        }
    }
}
