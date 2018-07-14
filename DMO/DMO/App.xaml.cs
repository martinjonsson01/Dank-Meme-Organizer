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

        private async Task SetUpDatabase()
        {
            using (var context = new MediaDataDatabaseContext())
            {
                // TODO: WARNING REMOVE THIS. IT CLEARS AND RESETS THE ENTIRE DATABASE.
                await context.Database.EnsureDeletedAsync();

                //await context.Database.MigrateAsync(); This database migration stuff is awesome but I can't use it because it requires a messed up EFCore tool not available for UWP.
                // Creates the database if it does not exist.
                await context.Database.EnsureCreatedAsync();

                // Create a dummy data and add it to database.
                var data = new ImageData(null)
                {
                    LastModified = DateTime.Now,
                    Labels = new List<Label>
                    {
                        new Label { Name = "Name1", Probability = 0.1f },
                        new Label { Name = "Name2", Probability = 65.5f },
                        new Label { Name = "Name3", Probability = 21.7f }
                    },
                    Title = "Title of image",
                };
                if (!await context.MediaDatas.ContainsAsync(data))
                    context.MediaDatas.Add(data);
                await context.SaveChangesAsync();

                foreach (var mediaData in context.MediaDatas)
                    Debug.WriteLine(mediaData.Title);
            }
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Activate)
            {
                if (args.Kind == ActivationKind.Protocol)
                {
                    // Extracts the authorization response URI from the arguments.
                    var protocolArgs = (ProtocolActivatedEventArgs)args;
                    var uri = protocolArgs.Uri;
                    Debug.WriteLine("Authorization Response: " + uri.AbsoluteUri);

                    // Opens the URI for "navigation" (handling) on the MainPage. TODO: Open Auth completed page here.
                    await NavigationService.NavigateAsync(typeof(Views.GalleryPage), SettingsService.Instance.FolderPath);
                    Window.Current.Activate();
                }
            }
            if (startKind == StartKind.Launch)
            {
                // Set up database.
                Debug.WriteLine("Setting up database...");
                var sw = new Stopwatch();
                sw.Start();
                await SetUpDatabase();
                sw.Stop();
                Debug.WriteLine($"Database setup! Elapsed time: {sw.ElapsedMilliseconds} ms");

                // If no folder path has been set, have the user select one.
                if (string.IsNullOrEmpty(SettingsService.Instance.FolderPath))
                    await NavigationService.NavigateAsync(typeof(Views.FolderSelectPage));
                else
                    await NavigationService.NavigateAsync(typeof(Views.GalleryPage), SettingsService.Instance.FolderPath);
            }
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
