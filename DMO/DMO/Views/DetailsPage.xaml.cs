using DMO.Extensions;
using DMO.Models;
using DMO.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Services.SerializationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DMO.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        private bool _image;
        private bool _gif;
        private bool _video;

        private bool _animFinished;

        public DetailsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region Stuff from ViewModel

            var vm = DataContext as DetailsPageViewModel;

            Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Navigated to {nameof(DetailsPageViewModel)}.");

            vm.MediaDataKey = e.Parameter?.ToString();

            var SerializationService = Template10.Services.SerializationService.SerializationService.Json;
            var mediaFile = App.Files[SerializationService.Deserialize(vm.MediaDataKey)?.ToString()];

            Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Media file loaded.");

            MediaData mediaData = null;
            foreach(var data in App.MediaDatas)
            {
                if (data.Meta.MediaFilePath == mediaFile.Path)
                    mediaData = data;
            }

            if (mediaFile.FileType == ".gif")
            {
                if (mediaData != null)
                    vm.ImageMediaData = mediaData;
                else
                    vm.ImageMediaData = new GifData(mediaFile);
            }
            else if (mediaFile.IsVideo())
            {
                vm.IsVideo = true;
                if (mediaData != null)
                    vm.VideoMediaData = mediaData;
                else
                    vm.VideoMediaData = new VideoData(mediaFile);
            }
            else
            {
                if (mediaData != null)
                    vm.ImageMediaData = mediaData;
                else
                    vm.ImageMediaData = new ImageData(mediaFile);

                var image = ImageElement.FindName("Media") as Image;
                var imageBitmap = new BitmapImage();
                image.Source = imageBitmap;
                var sw = new Stopwatch();
                sw.Start();
                // Open stream to file and apply as bitmap source.
                await imageBitmap.SetSourceAsync(await mediaFile.OpenReadAsync());
                sw.Stop();
                Debug.WriteLine($"Bitmap loaded. Took {sw.ElapsedMilliseconds} ms");
            }
            Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Media data loaded.");

            #endregion

            Frame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));

            var imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsImage1");
            var gifAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsGif1");
            var videoAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsVideo1");

            /* var compositor = ElementCompositionPreview.GetElementVisual(BackgroundPanel).Compositor;
             var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
             fadeAnimation.InsertKeyFrame(0, 0f);
             fadeAnimation.InsertKeyFrame(1, 1f);
             fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);
             fadeAnimation.Target = "Opacity";

             ElementCompositionPreview.SetImplicitShowAnimation(BackgroundPanel, fadeAnimation);*/

            if (imageAnimation != null)
            {
                _image = true;
                imageAnimation.Completed += (sender, arg) => _animFinished = true;
                ImageElement.ImageLoadedAction = () =>
                {
                    Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Detailed image loaded.");
                    imageAnimation.TryStart(ImageElement);
                };
            }
            if (gifAnimation != null)
            {
                _gif = true;
                gifAnimation.Completed += (sender, arg) => _animFinished = true;
                ImageElement.ImageLoadedAction = () =>
                {
                    gifAnimation.TryStart(ImageElement);
                };
            }
            if (videoAnimation != null)
            {
                _video = true;
                videoAnimation.Completed += (sender, arg) => _animFinished = true;
                MediaPlayerElement.VideoLoadedAction += async () =>
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => videoAnimation.TryStart(MediaPlayerElement));
                MediaPlayerElement.Detailed = true;
            }

            // Allow back-navigation if all animations are null.
            if (imageAnimation == null && gifAnimation == null && videoAnimation == null)
                _animFinished = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            /*if (!_animFinished)
            {
                e.Cancel = true;
                return;
            }*/

            if (_image)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsImage2", ImageElement);
            if (_gif)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsGif2", ImageElement);
            if (_video)
            {
                MediaPlayerElement.Detailed = false;
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsVideo2", MediaPlayerElement);
            }
        }
    }
}
