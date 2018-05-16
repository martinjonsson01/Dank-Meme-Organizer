using DMO.Services.SettingsServices;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Windows.Media.Core;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236
namespace DMO.Controls
{
    public sealed partial class MediaPlayerHover : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The event that is fired when any child property changes its value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        private MediaSource _mediaSource;

        public string MediaFileUid
        {
            get => GetValue(MediaFileUidProperty)?.ToString();
            set => SetValue(MediaFileUidProperty, value);
        }

        public static readonly DependencyProperty MediaFileUidProperty =
          DependencyProperty.Register(nameof(MediaFileUid), typeof(string), typeof(MediaPlayerHover), null);

        public BitmapImage Thumbnail
        {
            get => (BitmapImage)GetValue(ThumbnailProperty);
            set => SetValue(ThumbnailProperty, value);
        }

        public static readonly DependencyProperty ThumbnailProperty =
          DependencyProperty.Register(nameof(Thumbnail), typeof(BitmapImage), typeof(MediaPlayerHover), null);

        public bool IsLoaded => !Suspended;

        public bool Suspended
        {
            get => (bool)GetValue(SuspendedProperty);
            set
            {
                if (value && _mediaSource != null)
                {
                    MediaElement.Stop();
                    _mediaSource.Reset();
                }

                SetValue(SuspendedProperty, value);
            }
        }

        public static readonly DependencyProperty SuspendedProperty =
            DependencyProperty.Register(nameof(Suspended), typeof(bool), typeof(MediaPlayerHover), null);

        public TimeSpan Duration => MediaElement?.NaturalDuration.TimeSpan - MediaElement?.Position ?? new TimeSpan(0);

        public MediaPlayerHover()
        {
            InitializeComponent();
        }

        public async void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));

            if (name == nameof(MediaFileUid))
            {
                try
                {
                    // Apply volume.
                    MediaElement.Volume = SettingsService.Instance.MediaVolume;
                    // Get media file using UID.
                    var mediaFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(MediaFileUid);
                    _mediaSource = MediaSource.CreateFromStorageFile(mediaFile);
                    // Open stream to media file and set as source for video.
                    MediaElement.SetPlaybackSource(_mediaSource);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{MediaFileUid} : {e}");
                }
            }
        }
        
        private void MediaPlayer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PlaySymbol.Symbol = Symbol.Pause;
            MediaElement?.Play();
        }

        private void MediaPlayer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PlaySymbol.Symbol = Symbol.Play;
            MediaElement?.Stop();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlaySymbol.Symbol = Symbol.Play;
            MediaElement?.Stop();
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Create timer that updates the Duration property every 100 ms.
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (timerSender, arg) =>
            {
                OnPropertyChanged(nameof(Duration));
            };
            timer.Start();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine($"{MediaFileUid} : {e}");
        }
    }
}
