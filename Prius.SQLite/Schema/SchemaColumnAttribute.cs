using System;

namespace Prius.SqLite.Schema
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SchemaColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public System.Data.DbType DataType { get; private set; }
        public ColumnAttributes ColumnAttributes { get; set; }

        /// <summary>
        /// Constructs an attribute that specifies how to add this property to the database
        /// as a column.
        /// </summary>
        /// <param name="columnName">The name of this column in the database table</param>
        /// <param name="dataType">The type of data that will be stored in this column</param>
        /// <param name="columnAttributes">Additional charateristics for this column</param>
        public SchemaColumnAttribute(string columnName, System.Data.DbType dataType, ColumnAttributes columnAttributes)
        {
            ColumnName = columnName;
            DataType = dataType;
            ColumnAttributes = columnAttributes;
        }

        /// <summary>
        /// Constructs an attribute that specifies how to add this property to the database
        /// as a column.
        /// </summary>
        /// <param name="columnName">The name of this column in the database table</param>
        /// <param name="dataType">The type of data that will be stored in this column</param>
        public SchemaColumnAttribute(string columnName, System.Data.DbType dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
            ColumnAttributes = ColumnAttributes.None;
        }

        /// <summary>
        /// Constructs an attribute that specifies how to add this property to the database
        /// as a column.
        /// </summary>
        /// <param name="columnName">The name of this column in the database table</param>
        public SchemaColumnAttribute(string columnName)
        {
            ColumnName = columnName;
            DataType = System.Data.DbType.String;
            ColumnAttributes = ColumnAttributes.None;
        }
    }
}
