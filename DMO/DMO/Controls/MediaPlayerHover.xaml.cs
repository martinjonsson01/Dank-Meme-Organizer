using DMO.Services.SettingsServices;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Windows.Media.Core;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
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

        public string FileName
        {
            get => GetValue(FileNameProperty)?.ToString();
            set => SetValue(FileNameProperty, value);
        }

        public static readonly DependencyProperty FileNameProperty =
          DependencyProperty.Register(nameof(FileName), typeof(string), typeof(MediaPlayerHover), null);

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

            if (name == nameof(FileName))
            {
                if (MediaElement != null)
                {
                    // Apply volume.
                    MediaElement.Volume = SettingsService.Instance.MediaVolume;
                    // Get media file using file name.
                    var mediaFile = App.Files[FileName];
                    // Load thumbnail.
                    var thumbnail = await mediaFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                    // Apply poster source.
                    var thumbnailBitmap = new BitmapImage();
                    MediaElement.PosterSource = thumbnailBitmap;
                    await thumbnailBitmap.SetSourceAsync(thumbnail);
                    _mediaSource = MediaSource.CreateFromStorageFile(mediaFile);
                    // Open stream to media file and set as source for video.
                    MediaElement.SetPlaybackSource(_mediaSource);
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
            Debug.WriteLine($"{FileName} : {e}");
        }
    }
}
