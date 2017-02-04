using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.SqLite.SchemaUpdating
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
