using System.Collections.Generic;

namespace SQLite_Migrations
{
    /// <summary>
    /// Represents a table structure in SQLite
    /// </summary>
    internal class Table : List<ColumnInfo>
    {
        public string Name { get; set; }
    }
}
