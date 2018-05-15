using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace DMO.ViewModels
{
    public class ViewModelBase : Template10.Mvvm.ViewModelBase
    {
        public double TitleBarMargin { get; private set; } = 30;

        public ViewModelBase()
        {
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (titlebar, e) =>
            {
                TitleBarMargin = titlebar.Height;
                if (TitleBarMargin == 0)
                    TitleBarMargin = 30;
            };
        }
    }
}
