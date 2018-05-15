using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    public class SortDirectionIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SortDirection direction)
            {
                if (direction == SortDirection.Descending)
                    return new SymbolIcon(Symbol.Download);
            }
            return new SymbolIcon(Symbol.Up);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Symbol symbol)
            {
                if (symbol == Symbol.Up)
                    return SortDirection.Ascending;
            }
            return SortDirection.Descending;
        }
    }
}
