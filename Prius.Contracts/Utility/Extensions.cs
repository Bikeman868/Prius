using System;
using System.Collections.Generic;

namespace Prius.Contracts.Utility
{
    public static class Extensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.BaseType == typeof(ValueType);
        }

        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int index = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }
        
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }
    }
}
