using DMO_Model.GoogleAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

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

        public static Windows.UI.Color GetColor(this DMO_Model.GoogleAPI.Models.Color color)
        {
            var alpha01 = color.Alpha ?? 255.0f;
            return Windows.UI.Color.FromArgb((byte)alpha01, (byte)color.Red, (byte)color.Green, (byte)color.Blue);
        }

        public static Windows.UI.Color GetColor(this Likelihood likelihood)
        {
            switch(likelihood)
            {
                case Likelihood.VERY_LIKELY:
                    return Colors.Crimson;
                case Likelihood.LIKELY:
                    return Colors.Red;
                case Likelihood.POSSIBLE:
                    return Colors.LightCoral;
                case Likelihood.UNLIKELY:
                    return Colors.Green;
                case Likelihood.VERY_UNLIKELY:
                    return Colors.Lime;
                default:
                    return Colors.White;
            }
        }
    }
}
