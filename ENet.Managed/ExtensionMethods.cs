using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENet.Managed
{
    internal static class ExtensionMethods
    {
        public static bool OrderlessRemove<T>(this List<T> list, T item)
        {
            var index = list.FindIndex(p => p.Equals(item));
            if (index < 0) return false;
            list.OrderlessRemoveAt(index);
            return true;
        }

        public static void OrderlessRemoveAt<T>(this List<T> list, int index)
        {
            if (list.Count > 2)
            {
                var lastIndex = list.Count - 1;
                var last = list[lastIndex];
                list[index] = last;
                list.RemoveAt(lastIndex);
            }
            else
            {
                list.RemoveAt(index);
            }
        }
    }
}
