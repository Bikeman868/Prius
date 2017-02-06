using System;

namespace Prius.SqLite.Schema
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SchemaIndexAttribute : Attribute
    {
        public string IndexName { get; private set; }
        public IndexAttributes IndexAttributes { get; set; }

        /// <summary>
        /// Constructs an attribute that specifies to include this property in an index
        /// as a column.
        /// </summary>
        /// <param name="indexName">The name of the index to add this column to</param>
        /// <param name="indexAttributes">Additional charateristics for this index</param>
        public SchemaIndexAttribute(string indexName, IndexAttributes indexAttributes)
        {
            IndexName = indexName;
            IndexAttributes = indexAttributes;
        }

        /// <summary>
        /// Constructs an attribute that specifies to include this property in an index
        /// as a column.
        /// </summary>
        /// <param name="indexName">The name of the index to add this column to</param>
        public SchemaIndexAttribute(string indexName)
        {
            IndexName = indexName;
            IndexAttributes = IndexAttributes.None;
        }
    }
}
