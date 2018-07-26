using DMO.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class GenericMediaDataElement : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MediaData MediaData
        {
            get => (MediaData)GetValue(MediaDataProperty);
            set
            {
                if (value is ImageData ||
                    value is GifData)
                {
                    ImageMediaData = value;

                    LoadBitmapAsync(value);
                }

                if (value is VideoData)
                {
                    VideoMediaData = value;
                    IsVideo = true;
                }

                SetValue(MediaDataProperty, value);
            }
        }

        public static readonly DependencyProperty MediaDataProperty =
          DependencyProperty.Register(nameof(MediaData), typeof(MediaData), typeof(GenericMediaDataElement), null);

        public FastImage ImageElement { get; set; }

        public MediaPlayerHover VideoElement { get; set; }
        
        public bool IsVideo { get; set; }

        public bool IsImage => !IsVideo;

        public MediaData VideoMediaData { get; set; }

        public MediaData ImageMediaData { get; set; }

        public GenericMediaDataElement()
        {
            this.InitializeComponent();

            ImageElement = _imageElement;
            VideoElement = _mediaPlayerElement;
        }

        private async void LoadBitmapAsync(MediaData mediaData)
        {
            var image = ImageElement.FindName("Media") as Image;
            var imageBitmap = new BitmapImage();
            image.Source = imageBitmap;
            var sw = new Stopwatch();
            sw.Start();
            // Open stream to file and apply as bitmap source.
            await imageBitmap.SetSourceAsync(await mediaData.MediaFile.OpenReadAsync());
            sw.Stop();
            Debug.WriteLine($"Bitmap loaded. Took {sw.ElapsedMilliseconds} ms");

            // Update metadata about image size.
            mediaData.Meta.Height = imageBitmap.PixelHeight;
            mediaData.Meta.Width = imageBitmap.PixelWidth;
            mediaData.Meta.Height = imageBitmap.PixelHeight;
            mediaData.Meta.Width = imageBitmap.PixelWidth;
            ImageMediaData.Meta.Height = imageBitmap.PixelHeight;
            ImageMediaData.Meta.Width = imageBitmap.PixelWidth;
            RaisePropertyChanged(nameof(ImageMediaData.Meta));

            Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Media data loaded.");
        }

    }
}
