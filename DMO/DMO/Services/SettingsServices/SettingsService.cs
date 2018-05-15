using Microsoft.Toolkit.Uwp.UI;
using System;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;

namespace DMO.Services.SettingsServices
{
    public class SettingsService
    {
        public static SettingsService Instance { get; } = new SettingsService();
        Template10.Services.SettingsService.ISettingsHelper _helper;
        private SettingsService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public bool UseShellBackButton
        {
            get => _helper.Read<bool>(nameof(UseShellBackButton), true);
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.GetDispatcherWrapper().Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                });
            }
        }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Dark;
                var value = _helper.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse(value, out theme) ? theme : ApplicationTheme.Light;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get => _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2));
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        public string FolderPath
        {
            get => _helper.Read(nameof(FolderPath), null as string);
            set => _helper.Write(nameof(FolderPath), value);
        }

        public string SortBy
        {
            get => _helper.Read(nameof(SortBy), null as string);
            set => _helper.Write(nameof(SortBy), value);
        }

        public bool AutoPlayGifs
        {
            get => _helper.Read(nameof(AutoPlayGifs), true);
            set => _helper.Write(nameof(AutoPlayGifs), value);
        }

        public double MediaVolume
        {
            get => _helper.Read(nameof(MediaVolume), 0.1d);
            set => _helper.Write(nameof(MediaVolume), value);
        }

        public SortDirection SortDirection
        {
            get => _helper.Read(nameof(SortDirection), SortDirection.Ascending);
            set => _helper.Write(nameof(SortDirection), value);
        }
    }
}
