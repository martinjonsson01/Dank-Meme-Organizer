using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Data;

namespace DMO.Converters
{
    public class StartupTaskStateToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StartupTaskState state)
            {
                if (state == StartupTaskState.Enabled || state == StartupTaskState.EnabledByPolicy)
                    return "Application start with Windows is enabled";
            }
            return "Request to start application with windows";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
