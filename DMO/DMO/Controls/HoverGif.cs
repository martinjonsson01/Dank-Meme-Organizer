﻿using DMO.Services.SettingsServices;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236
namespace DMO.Controls
{
    public sealed partial class HoverGif : UserControl, INotifyPropertyChanged
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
          DependencyProperty.Register(nameof(FileName), typeof(string), typeof(HoverGif), null);

        public HoverGif()
        {
            InitializeComponent();
        }
        
        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (Image.Source is BitmapImage bitmap)
            {
                if (bitmap.IsAnimatedBitmap)
                {
                    if (!bitmap.IsPlaying)
                    {
                        bitmap.Play();
                    }
                }
            }
        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Image.Source is BitmapImage bitmap)
            {
                if (bitmap.IsAnimatedBitmap)
                {
                    bitmap.Stop();
                }
            }
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (SettingsService.Instance.AutoPlayGifs)
            {
                Root.PointerExited -= Image_PointerExited;
                return;
            }

            (Image.Source as BitmapImage)?.Stop();
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
