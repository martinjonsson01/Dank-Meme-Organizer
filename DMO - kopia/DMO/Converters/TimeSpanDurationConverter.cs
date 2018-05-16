using DMO.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    public class TimeSpanDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan?)
            {
                var duration = value as TimeSpan?;

                // Clamp ticks between max and min.
                var clampedDurationTicks = duration?.Ticks.Clamp(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
                // If ticks are null set as 0.
                return new DateTime(clampedDurationTicks ?? 0).ToString("mm:ss");
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
