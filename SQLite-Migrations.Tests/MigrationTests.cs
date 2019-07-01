using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite;

namespace SQLite_Migrations.Tests
{
    [TestClass]
    public class MigrationTests
    {
        private const string dbName = "test.db";
        private const string dbPath = "./DB";

        [TestInitialize]
        public void TestInitialise()
        {
            DeleteDatabase();
        }

        /// <summary>
        /// This test uses automatic migrations. Because they have been numbered with breaks in the sequence
        /// only Migration001 will execute.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanInitializeADatabase()
        {
            using (var cn = GetConnection())
            {
                var init = new DbInitialiser(cn, this.GetType().Assembly);
                await init.InitialiseDatabase();
            }
            using (var cn = GetConnection())
            {
                var init = new DbInitialiser(cn, this.GetType().Assembly);
                var tables = init.GetTableNames();
                Assert.IsTrue(tables.Contains("Person"));
                Assert.IsTrue(tables.Contains("Stuff"));
            }
        }

        /// <summary>
        /// This test uses pre-defined IMigration objects.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanSeedADatabase()
        {
            using (var cn = GetConnection())
            {
                var init = new DbInitialiser(cn, null);
                await init.InitialiseDatabase(new IMigration[]{new Migration101()});
            }

            using (var cn = GetConnection())
            {
                var people = cn.Query<Migration101.Person>("SELECT * FROM Person");
                Assert.AreEqual(2, people.Count);
                var stuff = cn.Query<Migration101.Stuff>("SELECT * FROM Stuff");
                Assert.AreEqual(4, stuff.Count);
            }
        }

        /// <summary>
        /// This test uses pre-defined IMigration objects however they are/must be sequential.
        /// In this case Migration201 and Migration 202
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CanModifyATable()
        {
            using (var cn = GetConnection())
            {
                var init = new DbInitialiser(cn, null);
                await init.InitialiseDatabase(new IMigration[] { new Migration201(), new Migration202()  });
            }

            using (var cn = GetConnection())
            {
                var init = new DbInitialiser(cn, null);
                var columnNames = init.GetColumnNames("Person");
                Assert.AreEqual(4, columnNames.Length);
            }
        }

        private void DeleteDatabase()
        {
            EnsureDbPath();
            var dbPathAndName = GetDbPathAndName();
            File.Delete(dbPathAndName);
        }

        private SQLiteConnection GetConnection()
        {
            EnsureDbPath();
            var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;
            return new SQLiteConnection(GetDbPathAndName(), flags, true);
        }

        private string GetDbPathAndName()
        {
            return Path.Combine(dbPath, dbName);
        }

        private void EnsureDbPath()
        {
            if (!Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);
        }

    }
}
