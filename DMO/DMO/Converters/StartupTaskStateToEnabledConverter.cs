using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    /// <summary>
    /// Will return true(enabled) when startup task is NOT enabled.
    /// Will return false(disabled) when startup task IS enabled.
    /// </summary>
    public class StartupTaskStateToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StartupTaskState state)
            {
                if (state == StartupTaskState.Enabled) return false;
                if (state == StartupTaskState.EnabledByPolicy) return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
