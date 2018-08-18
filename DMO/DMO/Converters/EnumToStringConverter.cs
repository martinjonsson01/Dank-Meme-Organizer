using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum e)
            {
                // Replaces underscores with spaces.
                // Makes enum string lower case.
                // Capitalizes first letter of each word.
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                        e.ToString()
                        .Replace('_', ' ')
                        .ToLower()
                    );
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
