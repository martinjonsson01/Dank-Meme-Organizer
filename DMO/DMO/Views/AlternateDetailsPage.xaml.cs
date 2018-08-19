using System;
using System.Diagnostics;
using DMO.Utility.Logging;
using DMO.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DMO.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlternateDetailsPage : Page
    {
        private bool _image;
        private bool _gif;
        private bool _video;

        private bool _animFinished;

        public AlternateDetailsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            #region Stuff from ViewModel

            var vm = DataContext as AlternateDetailsPageViewModel;
            
            vm.MediaDataKey = e.Parameter?.ToString();

            StorageFile mediaFile;
            // Log deserialization of media file.
            using (new DisposableLogger(DatabaseLog.DeserializationBegin, (sw) => DatabaseLog.DeserializationEnd(sw, 1)))
            {
                var SerializationService = Template10.Services.SerializationService.SerializationService.Json;
                mediaFile = App.Files[SerializationService.Deserialize(vm.MediaDataKey)?.ToString()];
            }

            var mediaData = App.Gallery.GetMediaDataFromPath(mediaFile.Path, App.Gallery.MediaDatas);

            vm.MediaData = mediaData;

            vm.RaisePropertyChanged(nameof(vm.MediaSource));

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

             ElementCompositionPreview.SetImplicitShowAnimation(BackgroundPanel, fadeAnimation);
            await Task.Delay(100);*/
            /*if (imageAnimation != null)
            {
                _image = true;
                imageAnimation.Completed += (sender, arg) => _animFinished = true;
                ImageElement.Loaded += (s, o) =>
                {
                    DeasdasdasdLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Detailed image loaded.");
                    imageAnimation.TryStart(ImageElement);

                    imageAnimation.Completed += (anim, obj) => vm.SearchForHigherResolutionOnlineAsync();
                };
            }
            if (gifAnimation != null)
            {
                _gif = true;
                gifAnimation.Completed += (sender, arg) => _animFinished = true;
                GenericMediaDataElement.ImageElement.ImageLoadedAction = () =>
                {
                    gifAnimation.TryStart(GenericMediaDataElement.ImageElement);

                    gifAnimation.Completed += (anim, obj) => vm.SearchForHigherResolutionOnlineAsync();
                };
            }
            if (videoAnimation != null)
            {
                _video = true;
                videoAnimation.Completed += (sender, arg) => _animFinished = true;
                GenericMediaDataElement.VideoElement.VideoLoadedAction += async () =>
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => videoAnimation.TryStart(GenericMediaDataElement.VideoElement));
                GenericMediaDataElement.VideoElement.Detailed = true;
            }

            // Pick up on property changes inside of GenericMediaDataElement.
            GenericMediaDataElement.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(GenericMediaDataElement.ImageMediaData.Meta))
                {
                    // Update width and height of local media datas based on new values of ImageMediaData.Meta.
                    mediaData.Meta.Height = GenericMediaDataElement.ImageMediaData.Meta.Height;
                    mediaData.Meta.Width = GenericMediaDataElement.ImageMediaData.Meta.Width;
                    vm.MediaData.Meta.Height = GenericMediaDataElement.ImageMediaData.Meta.Height;
                    vm.MediaData.Meta.Width = GenericMediaDataElement.ImageMediaData.Meta.Width;
                    vm.RaisePropertyChanged(nameof(vm.Dimensions));
                }
            };*/

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

            /*if (_image)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsImage2", GenericMediaDataElement.ImageElement);
            if (_gif)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsGif2", GenericMediaDataElement.ImageElement);
            if (_video)
            {
                GenericMediaDataElement.VideoElement.Detailed = false;
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsVideo2", GenericMediaDataElement.VideoElement);
            }*/
        }
    }
}
