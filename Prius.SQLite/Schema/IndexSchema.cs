namespace Prius.SQLite.Schema
{
    public class IndexSchema
    {
        public string IndexName { get; set; }
        public IndexAttributes Attributes { get; set; }
        public string[] ColumnNames { get; set; }
    }
}
