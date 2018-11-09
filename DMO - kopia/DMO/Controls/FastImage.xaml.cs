using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using DMO.Utility.Logging;
using Template10.Mvvm;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DMO.Controls
{
    public sealed partial class FastImage : UserControl, INotifyPropertyChanged
    {
        private ContentDialog _deleteConfirmDialog;

        /// <summary>
        /// The event that is fired when any child property changes its value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public string FileName
        {
            get => GetValue(FileNameProperty)?.ToString();
            set => SetValue(FileNameProperty, value);
        }

        public static readonly DependencyProperty FileNameProperty =
          DependencyProperty.Register(nameof(FileName), typeof(string), typeof(FastImage), null);

        public Action ImageLoadedAction
        {
            get => GetValue(ImageLoadedProperty) as Action;
            set => SetValue(ImageLoadedProperty, value);
        }

        public static readonly DependencyProperty ImageLoadedProperty =
          DependencyProperty.Register(nameof(ImageLoadedAction), typeof(Action), typeof(FastImage), null);

        #region Commands

        public DelegateCommand CopyCommand
            => new DelegateCommand(async () =>
            {
                var dataPackage = new DataPackage();
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(App.Files[FileName]));
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
                if(FocusManager.FindNextElement(FocusNavigationDirection.Next) is Control c)
                {
                    // Shift focus to textbox.
                    c.Focus(FocusState.Programmatic);
                }
            });

        #endregion

        public FastImage()
        {
            this.InitializeComponent();
            Media.ImageOpened += (sender, e) => ImageLoadedAction?.Invoke();
        }

        public async void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));

            if (name == nameof(FileName))
            {
                if (App.Files.ContainsKey(FileName))
                {
                    // Get media file using file name.
                    /*var path = App.Files[FileName].Path;
                    var mediaFile = await StorageFile.GetFileFromPathAsync(path);*/
                    var mediaFile = App.Files[FileName];
                    // Apply image source.
                    var imageBitmap = new BitmapImage();
                    Media.Source = imageBitmap;
                    // Open stream to file and apply as bitmap source.
                    try
                    {
                        await imageBitmap.SetSourceAsync(await mediaFile.OpenReadAsync());
                        //await imageBitmap.SetSourceAsync(await mediaFile.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)App.Gallery.ImageSize, ThumbnailOptions.UseCurrentScale));

                        // Get mediaData from gallery.
                        var mediaData = App.Gallery.MediaDatas.FirstOrDefault(data => data.Meta.MediaFilePath.Equals(mediaFile.Path));
                        if (mediaData != null )
                        {
                            // Update meta height and width.
                            mediaData.Meta.Height = imageBitmap.PixelHeight;
                            mediaData.Meta.Width = imageBitmap.PixelWidth;
                        }
                    }
                    catch (Exception e)
                    {
                        // Log Exception.
                        LifecycleLog.Exception(e);
                    }
                }
            }
        }
    }
}
