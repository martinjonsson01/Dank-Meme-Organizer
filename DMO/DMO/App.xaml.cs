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
using System.Linq;
using Windows.Storage;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using DMO.Models;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Data.Sqlite;
using System;
using System.Diagnostics;
using DMO.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using Windows.Security.Authentication.Web;
using System.Net.Http;
using DMO.GoogleAPI;
using DMO_Model.Models;
using DMO_Model.GoogleAPI.Models;

namespace DMO
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    public sealed partial class App : BootStrapper
    {
        public static Dictionary<string, StorageFile> Files = new Dictionary<string, StorageFile>();
        
        public static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

        public static Gallery Gallery;

        public static string VisionAPIKey = "AIzaSyBp1PKmeQWeMQ2DyKCv1dfBUO1aFgReico";

        /// <summary>
        /// These are the MediaDatas loaded from the database. Need to be processed before used.
        /// </summary>
        public static  List<MediaData> MediaDatas = new List<MediaData>();

        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #if DEBUG
            DebugSettings.EnableFrameRateCounter = true;
            #endif

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

        private async Task SetUpAndLoadFromDatabase()
        {
            using (var context = new MediaMetaDatabaseContext())
            {
                // TODO: WARNING REMOVE THIS. IT CLEARS AND RESETS THE ENTIRE DATABASE.
                //await context.Database.EnsureDeletedAsync();

                // Creates and/or migrates the database if it does not exist/is not up to date.
                await context.Database.MigrateAsync();

                var metas = await context.GetAllMetadatasAsync();
                foreach (var meta in metas)
                {
                    var mediaData = await MediaData.CreateFromMediaMetadataAsync(meta);
                    MediaDatas.Add(mediaData);
                }
            }
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Activate)
            {
                if (args.Kind == ActivationKind.Protocol)
                {
                    // Opens the URI for "navigation" (handling) on the GalleryPage. TODO: Open Auth completed page here.
                    await NavigationService.NavigateAsync(typeof(Views.GalleryPage), SettingsService.Instance.FolderPath);
                    Window.Current.Activate();
                }
            }
            if (startKind == StartKind.Launch)
            {
                // Enable prelaunch.
                TryEnablePrelaunch();

                // Set up database.
                Debug.WriteLine("Setting up database...");
                var sw = new Stopwatch();
                sw.Start();
                await SetUpAndLoadFromDatabase();
                sw.Stop();
                Debug.WriteLine($"Database setup! Elapsed time: {sw.ElapsedMilliseconds} ms");

                // If MediaDatas have been loaded from database then open GalleryPage using those.
                if (MediaDatas.Count > 0)
                {
                    await NavigationService.NavigateAsync(typeof(Views.GalleryPage), nameof(MediaDatas));
                    return;
                }

                // If no folder path has been set, have the user select one.
                if (string.IsNullOrEmpty(SettingsService.Instance.FolderPath))
                    await NavigationService.NavigateAsync(typeof(Views.FolderSelectPage));
                else
                    await NavigationService.NavigateAsync(typeof(Views.GalleryPage));
            }
        }

        private static void TryEnablePrelaunch()
        {
            CoreApplication.EnablePrelaunch(true);
        }

        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }

        protected override INavigationService CreateNavigationService(Frame frame)
        {
            var navService = base.CreateNavigationService(frame);
            navService.Frame.ContentTransitions?.Clear();
            navService.Frame.ContentTransitions?.Add(new NavigationThemeTransition { DefaultNavigationTransitionInfo = new ContinuumNavigationTransitionInfo() });
            return navService;
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
