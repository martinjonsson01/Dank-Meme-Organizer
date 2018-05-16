using DMO.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class FolderSelectViewModel : ViewModelBase
    {
        #region Public Properties
        
        public GalleryFolderChooser GalleryFolder { get; set; }

        #endregion

        #region Constructor

        public FolderSelectViewModel()
        {
            GalleryFolder = new GalleryFolderChooser();
        }

        #endregion

        #region Public Methods

        public async void PickFolder()
        {
            await GalleryFolder.ChooseFolder();
        }

        public void GotoGalleryPage()
        {
            NavigationService.Navigate(typeof(Views.GalleryPage), GalleryFolder);
            NavigationService.ClearHistory();
        }

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            return base.OnNavigatingFromAsync(args);
        }
        
        #endregion
    }
}
