using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.FileProperties;
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

        public FastImage()
        {
            this.InitializeComponent();
        }

        public async void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));

            if (name == nameof(FileName))
            {
                // Get media file using file name.
                var mediaFile = App.Files[FileName];
                // Apply image source.
                var imageBitmap = new BitmapImage();
                Image.Source = imageBitmap;
                // Open stream to file and apply as bitmap source.
                await imageBitmap.SetSourceAsync(await mediaFile.OpenReadAsync());
            }
        }

    }
}
