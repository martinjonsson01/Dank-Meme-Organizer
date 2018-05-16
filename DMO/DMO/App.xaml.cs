using System.Threading.Tasks;
using DMO.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Common;
using Windows.UI.Xaml.Data;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.ApplicationModel;
using Windows.Storage.AccessCache;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.Storage;

namespace DMO
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    public sealed partial class App : BootStrapper
    {
        public static Dictionary<string, StorageFile> Files = new Dictionary<string, StorageFile>();

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App Settings

            // some settings must be set in app.constructor
            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;

            #endregion
            
            // Clear FutureAccessList as it has a max limit of 1000.
            ClearFutureAccessList();
        }
        
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here
            SettingsService.Instance.FolderPath = null;
            // If no folder path has been set, have the user select one.
            if (string.IsNullOrEmpty(SettingsService.Instance.FolderPath))
                await NavigationService.NavigateAsync(typeof(Views.FolderSelectPage));
            else
                await NavigationService.NavigateAsync(typeof(Views.GalleryPage), SettingsService.Instance.FolderPath);
        }

        private static void ClearFutureAccessList()
        {
            var toRemove = new List<string>();
            foreach (var entry in StorageApplicationPermissions.FutureAccessList.Entries)
            {
                if (entry.Token != "gallery")
                    toRemove.Add(entry.Token);
            }
            foreach (var tokenRemove in toRemove)
            {
                StorageApplicationPermissions.FutureAccessList.Remove(tokenRemove);
            }
        }

    }
}
