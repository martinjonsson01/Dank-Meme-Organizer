using System;
using System.Collections.Generic;
using System.Text;

namespace DMO_Model.Utility
{
    public static class Extensions
    {
        public static bool ListEquals<T>(this IList<T> list, IList<T> other)
        {
            if (other is null) return false;
            if (list.Count != other.Count) return false;

            for (var i = 0; i < list.Count; i++)
            {
                var listObj = list[i];
                var otherObj = other[i];

                if (!listObj.Equals(otherObj))
                    return false;
            }

            return true;
        }
    }
}
