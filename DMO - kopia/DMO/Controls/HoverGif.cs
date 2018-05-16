using DMO.Services.SettingsServices;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236
namespace DMO.Controls
{
    public sealed partial class HoverGif : UserControl
    {
        public BitmapImage GifBitmap
        {
            get => (BitmapImage)GetValue(GifBitmapProperty);
            set => SetValue(GifBitmapProperty, value);
        }

        public static readonly DependencyProperty GifBitmapProperty =
          DependencyProperty.Register(nameof(BitmapImage), typeof(Uri), typeof(HoverGif), null);
        
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
    }
}
