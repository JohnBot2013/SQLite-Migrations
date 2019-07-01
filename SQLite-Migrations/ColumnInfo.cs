namespace SQLite_Migrations
{
    /// <summary>
    /// Represents the column details of a table in SQLite
    /// </summary>
    internal class ColumnInfo
    {
        public int cid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int notnull { get; set; }
        public string dflt_value { get; set; }
        public int pk { get; set; }
    }
}
