using DMO.Services.SettingsServices;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236
namespace DMO.Controls
{
    public sealed partial class MediaPlayerHover : UserControl, INotifyPropertyChanged
    {
        private ContentDialog _deleteConfirmDialog;

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

        public Action VideoLoadedAction
        {
            get => GetValue(VideoLoadedActionProperty) as Action;
            set => SetValue(VideoLoadedActionProperty, value);
        }

        public static readonly DependencyProperty VideoLoadedActionProperty =
          DependencyProperty.Register(nameof(VideoLoadedAction), typeof(Action), typeof(MediaPlayerHover), null);

        public TimeSpan Duration => MediaElement?.NaturalDuration.TimeSpan - MediaElement?.Position ?? new TimeSpan(0);

        public bool Detailed
        {
            set
            {
                if (MediaElement != null)
                {
                    MediaElement.AreTransportControlsEnabled = value;
                    MediaElement.AutoPlay = value;
                }
            }
        }

        public static readonly DependencyProperty DetailedProperty =
            DependencyProperty.Register(nameof(Detailed), typeof(bool), typeof(MediaPlayerHover), null);

        #region Commands

        public DelegateCommand CopyCommand
            => new DelegateCommand(async () =>
            {
                var dataPackage = new DataPackage();
                dataPackage.SetStorageItems(new[] { App.Files[FileName] });
                try
                {
                    Clipboard.SetContent(dataPackage);
                }
                catch (Exception ex)
                {
                    // Copying data to Clipboard can potentially fail - for example, if another application is holding Clipboard open.
                    var failDialog = new ContentDialog()
                    {
                        Title = "Try again",
                        Content = "Error copying content to Clipboard: " + ex.Message + ".",
                        PrimaryButtonText = "Ok",
                    };
                    await failDialog.ShowAsync();
                }
            });

        public DelegateCommand DeleteConfirmCommand
            => new DelegateCommand(async () =>
            {
                _deleteConfirmDialog = new ContentDialog()
                {
                    Title = "Delete?",
                    Content = "Cannot be undone.",
                    PrimaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    CloseButtonText = "Delete",
                    CloseButtonCommand = DeleteCommand,
                };

                await _deleteConfirmDialog.ShowAsync();
            });

        public DelegateCommand DeleteCommand
            => new DelegateCommand(async () =>
            {
                if (App.Gallery.IsEvaluating)
                {
                    _deleteConfirmDialog?.Hide();
                    var errorDialog = new ContentDialog()
                    {
                        Title = "Cannot delete file.",
                        Content = "Please wait for image evaluation to finish before deleting any files.",
                        PrimaryButtonText = "Ok",
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                var mediaFile = App.Files[FileName];
                App.Gallery.RemoveFile(mediaFile.Path);
                await mediaFile.DeleteAsync();
            });

        public DelegateCommand RenameCommand
            => new DelegateCommand(async () =>
            {
                // Wait 100ms to allow context menu to close.
                await Task.Delay(100);
                // Find the next element(the textbox below).
                if (FocusManager.FindNextElement(FocusNavigationDirection.Next) is Control c)
                {
                    // Shift focus to textbox.
                    c.Focus(FocusState.Programmatic);
                }
            });

        #endregion

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
                    // TODO: MediaElement becomes null between the await-s, try reinstating a new one when it does that.
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

        private async void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                if (App.Files.ContainsKey(FileName))
                {
                    // Apply volume.
                    MediaElement.Volume = SettingsService.Instance.MediaVolume;
                    // Get media file using file name.
                    var mediaFile = App.Files[FileName];
                    // Load thumbnail.
                    var thumbnail = await mediaFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                    // TODO: MediaElement becomes null between the await-s, try reinstating a new one when it does that.
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
            Play();
        }

        private void MediaPlayer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Stop();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Play()
        {
            PlaySymbol.Symbol = Symbol.Pause;
            MediaElement?.Play();
        }

        private void Stop()
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

            VideoLoadedAction?.Invoke();
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine($"{FileName} : {e}");
        }
    }
}
