using System;

namespace Prius.SqLite.Schema
{
    [Flags]
    public enum IndexAttributes
    {
        /// <summary>
        /// Indicates no special attributes for the index
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the combined value of all columns in this index must be unique
        /// </summary>
        Unique = 1,
    }
}
