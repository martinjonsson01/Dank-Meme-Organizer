using DMO.Database;
using DMO.Utility;
using DMO.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
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

        public bool AutoPlayGifs
        {
            get => _settings.AutoPlayGifs;
            set => _settings.AutoPlayGifs = value;
        }

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

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
            }
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
