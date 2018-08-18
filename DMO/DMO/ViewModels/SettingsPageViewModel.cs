using DMO.Database;
using DMO.Utility;
using DMO.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace DMO.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
    }

    public class SettingsPartViewModel : ViewModelBase
    {
        Services.SettingsServices.SettingsService _settings;
        StartupTask _startupTask;

        public bool AutoPlayGifs
        {
            get => _settings.AutoPlayGifs;
            set => _settings.AutoPlayGifs = value;
        }

        public StartupTaskState StartupTaskState => _startupTask?.State ?? StartupTaskState.Disabled;
        
        DelegateCommand _changeFolderCommand;
        public DelegateCommand ChangeFolderCommand
            => _changeFolderCommand ?? (_changeFolderCommand = new DelegateCommand(async () =>
            {
                _settings.FolderPath = null;
                // Clear database of old gallery data.
                using (var context = new MediaMetaDatabaseContext())
                {
                    context.DeleteAllMetadatas();
                }
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                await nav.NavigateAsync(typeof(FolderSelectPage));
            }));

        DelegateCommand _enableStartupTaskCommand;
        public DelegateCommand EnableStartupTaskCommand
            => _enableStartupTaskCommand ?? (_enableStartupTaskCommand = new DelegateCommand(async () =>
            {
                // Get startup task using Task Id from appxmanifest if not already gotten.
                if (_startupTask == null)
                    _startupTask = await StartupTask.GetAsync("DankMemeOrganizerStartupTask");

                // Handle result.
                switch (_startupTask.State)
                {
                    case StartupTaskState.Disabled:
                        // Task is disabled but can be enabled.
                        var newState = await _startupTask.RequestEnableAsync();
                        Debug.WriteLine($"Request to enable startup, result = {newState}");
                        break;
                    case StartupTaskState.DisabledByUser:
                        // Task is disabled and user must enable it manually.
                        var disabledByUserDialog = new MessageDialog(
                            "You have disabled this app's ability to run " +
                            "as soon as you sign in, but if you change your mind, " +
                            "you can enable this in the Startup tab in Task Manager.",
                            "Dank Meme Organizer Startup Task");
                        await disabledByUserDialog.ShowAsync();
                        break;
                    case StartupTaskState.DisabledByPolicy:
                        var disabledByPolicyDialog = new MessageDialog(
                            "Startup disabled by group policy, or not supported on this device.",
                            "Dank Meme Organizer Startup Task");
                        await disabledByPolicyDialog.ShowAsync();
                        break;
                    case StartupTaskState.Enabled:
                        var enabledDialog = new MessageDialog(
                            "Startup task is already enabled. " + 
                            "Dank Meme Organizer will start when you sign into Windows.",
                            "Dank Meme Organizer Startup Task");
                        await enabledDialog.ShowAsync();
                        break;
                }
                // Update UI with a propertychanged event.
                RaisePropertyChanged(nameof(StartupTaskState));
            }));

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
                // Get startup task asynchronously using Task Id from appxmanifest.
                GetStartupService();
            }
        }

        private async void GetStartupService()
        {
            _startupTask = await StartupTask.GetAsync("DankMemeOrganizerStartupTask");
        }
    }

    public class AboutPartViewModel : ViewModelBase
    {
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public Uri RateMe => new Uri("http://aka.ms/template10");
    }
}
