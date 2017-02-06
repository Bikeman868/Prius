using System.Collections.Generic;

namespace Prius.SqLite.Schema
{
    public class TableSchema
    {
        public string RepositoryName { get; set; }
        public string TableName { get; set; }
        public IList<ColumnSchema> Columns { get; set; }
        public IList<IndexSchema> Indexes { get; set; }
    }
}
