using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    public class TopThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double leftMargin = 0;
            double topMargin = 0;
            double rightMargin = 0;
            double bottomMargin = 0;
            if (value is double margin)
            {
                topMargin = margin;
                if (parameter is string parameters)
                {
                    var margins = parameters.Split(' ');
                    if (margins.Length == 4)
                    {
                        double.TryParse(margins[0], out leftMargin);
                        double.TryParse(margins[2], out rightMargin);
                        double.TryParse(margins[3], out bottomMargin);
                    }
                }
            }
            return new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
