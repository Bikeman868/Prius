namespace Prius.SQLite.Schema
{
    public class ColumnSchema
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public ColumnAttributes Attributes { get; set; }
    }
}
