using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Orm.Utility
{
    /// <summary>
    /// Extension methods for the Type class
    /// </summary>
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.BaseType == typeof(ValueType);
        }
    }
}
