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
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using DMO.Models;

namespace DMO
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    public sealed partial class App : BootStrapper
    {
        public static Dictionary<string, StorageFile> Files = new Dictionary<string, StorageFile>();

        public static Gallery Gallery;

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            DebugSettings.EnableFrameRateCounter = true;

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
            
            // If no folder path has been set, have the user select one.
            if (string.IsNullOrEmpty(SettingsService.Instance.FolderPath))
                await NavigationService.NavigateAsync(typeof(Views.FolderSelectPage));
            else
                await NavigationService.NavigateAsync(typeof(Views.GalleryPage), SettingsService.Instance.FolderPath);
        }

        protected override INavigationService CreateNavigationService(Frame frame)
        {
            return base.CreateNavigationService(frame);
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
