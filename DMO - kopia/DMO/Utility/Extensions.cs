using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }

        public static bool ListEquals<T>(this IList<T> list, IList<T> other)
        {
            if (other is null)
                return false;
            if (list.Count != other.Count)
                return false;
            
            for (var i = 0; i < list.Count; i++)
            {
                var listObj = list[i];

                if (!other.Contains(listObj))
                    return false;
            }

            return true;
        }

    }
}
