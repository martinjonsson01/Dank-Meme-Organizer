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
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.ExtendedExecution;
using DMO.Utility.Logging;
using System.Diagnostics.Tracing;
using DMO.Utility;
using Windows.ApplicationModel.Background;

namespace DMO
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    public sealed partial class App : BootStrapper
    {
        public static Dictionary<string, StorageFile> Files = new Dictionary<string, StorageFile>();
        
        public static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

        public static readonly HttpClient HttpClientNoRedirect = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });

        public static Gallery Gallery;

        public static string VisionAPIKey = "AIzaSyBp1PKmeQWeMQ2DyKCv1dfBUO1aFgReico";

        /// <summary>
        /// These are the MediaDatas loaded from the database. Need to be processed before used.
        /// </summary>
        public static  List<MediaData> MediaDatas = new List<MediaData>();

        /// <summary>
        /// The <see cref="MediaMetadata"/> taken directly from the database. Don't mess with these, they are supposed to be untainted.
        /// </summary>
        public static List<MediaMetadata> DatabaseMetaDatas = new List<MediaMetadata>();

        private ExtendedExecutionSession session;

        private static StorageFileEventListener informationListener;

        private static StorageFileEventListener verboseListener;

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

            // Initialize logger.
            informationListener = new StorageFileEventListener("info");
            verboseListener = new StorageFileEventListener("verbose");
            // Enable events for loggers.
            informationListener.EnableEvents(EventLog.Log, EventLevel.Informational);
            verboseListener.EnableEvents(EventLog.Log, EventLevel.LogAlways);

			// Handle unhandled events.
			UnhandledException += App_UnhandledException;

            // Clear FutureAccessList as it has a max limit of 1000.
            ClearFutureAccessList();

            // Log startup.
            LifecycleLog.AppStarting();
        }

		private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			LifecycleLog.Exception(e.Exception);
		}

		private async Task SetUpAndLoadFromDatabase()
        {
            using (var context = new MediaMetaDatabaseContext())
            {
                // Creates and/or migrates the database if it does not exist/is not up to date.
                await context.Database.MigrateAsync();

                // Fetch metadatas from database.
                DatabaseMetaDatas = await context.GetAllMetadatasAsync();

                using(new DisposableLogger(DatabaseLog.CreateMediaDatasBegin, DatabaseLog.CreateMediaDatasEnd))
                {
                    // Turn metadatas into mediadatas.
                    foreach (var meta in DatabaseMetaDatas)
                    {
                        var mediaData = await MediaData.CreateFromMediaMetadataAsync(meta);
                        MediaDatas.Add(mediaData);
                    }
                }
            }
        }

        public override Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        {
            // Log prelaunch.
            LifecycleLog.AppPrelaunch();

            runOnStartAsync = true;
            return Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // Log OnStart.
            LifecycleLog.AppOnStart(startKind, args);
            // CoreApplication.EnablePrelaunch was introduced in Windows 10 version 1607
            var canEnablePrelaunch = Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

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
                if (canEnablePrelaunch)
                    TryEnablePrelaunch();

                // End the Extended Execution Session.
                ClearExtendedExecution();

                using (session = new ExtendedExecutionSession
                {
                    Reason = ExtendedExecutionReason.Unspecified,
                    Description = "Loading Memes from database"
                })
                {
                    // Register Revoked listener.
                    session.Revoked += SessionRevoked;

                    var accessStatus = BackgroundExecutionManager.GetAccessStatus();
                    if (accessStatus != BackgroundAccessStatus.AlwaysAllowed)
                    {
                        // Request background access.
                        var accessGranted = await BackgroundExecutionManager.RequestAccessKindAsync(BackgroundAccessRequestKind.AlwaysAllowed, "To allow faster launch performance");
                    }
                    // Request extension. This is done so that if the application can finish loading data
                    // from database when prelaunched or minimized (suspended prematurely).
                    var result = await session.RequestExtensionAsync();
                    LifecycleLog.ExtensionRequestResult(result);

                    if (result == ExtendedExecutionResult.Denied)
                    {
                        session.Dispose();
                        // TODO: Notify user of extension result denied.
                    }

                    // Set up database.
                    using (new DisposableLogger(DatabaseLog.LoadBegin, DatabaseLog.LoadEnd))
                    {
                        if (!(args.PreviousExecutionState == ApplicationExecutionState.Suspended && MediaDatas.Count > 0))
                            await SetUpAndLoadFromDatabase();
                    }

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
        }

        private void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
                session = null;
            }
        }

        private static void TryEnablePrelaunch()
        {
            CoreApplication.EnablePrelaunch(true);
        }

        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            // Log suspension.
            LifecycleLog.AppSuspending();

            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }
        
        protected override INavigationService CreateNavigationService(Frame frame)
        {
            var navService = base.CreateNavigationService(frame);
            navService.Frame.ContentTransitions?.Clear();
            navService.Frame.ContentTransitions?.Add(new NavigationThemeTransition { DefaultNavigationTransitionInfo = new ContinuumNavigationTransitionInfo() });

            frame.NavigationFailed += (obj, e) =>
            {
                // Log NavigationException.
                LifecycleLog.NavigationException(e);
                // Set handled to true so the application doesn't crash.
                e.Handled = true;
            };
            navService.FrameFacade.BackRequested += async (sender, backArgs) =>
            {
                // Handle event so this is the only place handling it.
                backArgs.Handled = true;
                
                // Perform navigation on main thread.
                await navService.Dispatcher.DispatchAsync(() =>
                {
                    try
                    {
                        // Go back when on main thread.
                        navService.FrameFacade.GoBack();
                    }
                    catch (Exception e)
                    {
                        // Log Exception.
                        LifecycleLog.Exception(e);
                    }
                },
                0, CoreDispatcherPriority.Normal);
            };

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

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            await NavigationService.Dispatcher.DispatchAsync(() =>
            {
                // Log reason.
                LifecycleLog.ExtensionRevoked(args.Reason);

                ClearExtendedExecution();
            }, 0, CoreDispatcherPriority.Normal);
        }

    }
}
