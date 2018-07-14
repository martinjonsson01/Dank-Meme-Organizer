using DMO.Extensions;
using DMO.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class DetailsPageViewModel : ViewModelBase
    {
        #region Public Properties

        public string MediaDataKey { get; set; }

        public bool IsVideo { get; set; }

        public bool IsImage => !IsVideo;

        public MediaData VideoMediaData { get; set; }

        public MediaData ImageMediaData { get; set; }

        #endregion

        #region Constructor

        public DetailsPageViewModel()
        {
            
        }

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(MediaDataKey)] = MediaDataKey;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

    }
}
