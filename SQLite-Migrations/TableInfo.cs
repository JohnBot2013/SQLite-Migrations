namespace SQLite_Migrations
{
    /// <summary>
    /// Used to hold table information from SQLite schema
    /// </summary>
    internal class TableInfo
    {
        public string type { get; set; }
        public string name { get; set; }
        public string tbl_name { get; set; }
        public int rootpage { get; set; }
        public string sql { get; set; }
    }
}
