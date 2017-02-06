using System;

namespace Prius.SqLite.Schema
{
    [Flags]
    public enum ColumnAttributes
    {
        /// <summary>
        /// Indicates no special attributes for the column
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the database engine should not store null values in thie column
        /// </summary>
        NotNull = 1,

        /// <summary>
        /// Indicates that a unique index should be created for this column
        /// </summary>
        Unique = 2,

        /// <summary>
        /// Indicates that each row in the table should set this to the next
        /// numberic value. Only applies to integer columns
        /// </summary>
        AutoIncrement = 4,

        /// <summary>
        /// A primary index should be created for this column. Implies that the
        /// column is unique
        /// </summary>
        Primary = 11,

        /// <summary>
        /// Indicates that this is the primary key column that auto-increments
        /// on each insert.
        /// </summary>
        UniqueKey = 15

    }
}
