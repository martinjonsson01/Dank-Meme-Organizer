using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

        public DetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsImage1");
            var gifAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsGif1");
            var videoAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsVideo1");

            if (imageAnimation != null)
            {
                _image = true;
                ImageElement.ImageLoadedAction = () => imageAnimation.TryStart(ImageElement);
            }
            if (gifAnimation != null)
            {
                _gif = true;
                ImageElement.ImageLoadedAction = () => gifAnimation.TryStart(ImageElement);
            }
            if (videoAnimation != null)
            {
                _video = true;
                MediaPlayerElement.VideoLoadedAction += async () => 
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => videoAnimation.TryStart(MediaPlayerElement));
                MediaPlayerElement.Detailed = true;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

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
