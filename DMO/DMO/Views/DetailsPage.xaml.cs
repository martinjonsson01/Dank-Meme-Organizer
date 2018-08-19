using System;
using System.Diagnostics;
using System.Linq;
using DMO.Utility;
using DMO.Utility.Logging;
using DMO.ViewModels;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private bool _animFinished;

        public DetailsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var bitmapLoadLogger = new DisposableLogger(UILog.BitmapLoadBegin, UILog.BitmapLoadEnd);

            #region Stuff from ViewModel

            var vm = DataContext as DetailsPageViewModel;

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

            // Set ViewBox, GenericMediaDataElement, VideoElement and ImageElement size to viewbox size or native size.
            MediaScrollViewer.UpdateLayout();
            MediaViewBox.Width = Math.Min(MediaScrollViewer.ViewportWidth, mediaData.Meta.Width);
            MediaViewBox.Height = Math.Min(MediaScrollViewer.ViewportHeight, mediaData.Meta.Height);
            GenericMediaDataElement.Width = Math.Min(MediaScrollViewer.ViewportWidth, mediaData.Meta.Width);
            GenericMediaDataElement.Height = Math.Min(MediaScrollViewer.ViewportHeight, mediaData.Meta.Height);
            GenericMediaDataElement.ImageElement.Width = Math.Min(MediaScrollViewer.ViewportWidth, mediaData.Meta.Width);
            GenericMediaDataElement.ImageElement.Height = Math.Min(MediaScrollViewer.ViewportHeight, mediaData.Meta.Height);
            GenericMediaDataElement.VideoElement.Width = Math.Min(MediaScrollViewer.ViewportWidth, mediaData.Meta.Width);
            GenericMediaDataElement.VideoElement.Height = Math.Min(MediaScrollViewer.ViewportHeight, mediaData.Meta.Height);
            if (imageAnimation != null)
            {
                _image = true;
                imageAnimation.Completed += (sender, arg) => _animFinished = true;
                // Start connected animation.
                imageAnimation.TryStart(GenericMediaDataElement.ImageElement);

                // Set up image loaded action.
                GenericMediaDataElement.ImageElement.ImageLoadedAction = () =>
                {
                    ZoomToFit();
                    App.Current.NavigationService.Frame.SizeChanged += MediaScrollViewer_SizeChanged;
                    MediaScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, ScrollViewerZoomFactorChanged);

                    // Log bitmap load.
                    bitmapLoadLogger.Dispose();

                    imageAnimation.Completed += (anim, obj) => vm.SearchForHigherResolutionOnlineAsync();
                };
            }
            if (gifAnimation != null)
            {
                _gif = true;
                gifAnimation.Completed += (sender, arg) => _animFinished = true;
                // Start connected animation.
                gifAnimation.TryStart(GenericMediaDataElement.ImageElement);
                // Set up image loaded action.
                GenericMediaDataElement.ImageElement.ImageLoadedAction = () =>
                {
                    ZoomToFit();
                    App.Current.NavigationService.Frame.SizeChanged += MediaScrollViewer_SizeChanged;
                    MediaScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, ScrollViewerZoomFactorChanged);

                    // Log bitmap load.
                    bitmapLoadLogger.Dispose();

                    gifAnimation.Completed += (anim, obj) => vm.SearchForHigherResolutionOnlineAsync();
                };
            }
            if (videoAnimation != null)
            {
                _video = true;
                videoAnimation.Completed += (sender, arg) => _animFinished = true;
                // Start connected animation on UI thread.
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => videoAnimation.TryStart(GenericMediaDataElement.VideoElement));
                // Set up video loaded action.
                GenericMediaDataElement.VideoElement.VideoLoadedAction += () =>
                {
                    ZoomToFit();
                    App.Current.NavigationService.Frame.SizeChanged += MediaScrollViewer_SizeChanged;
                    MediaScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ZoomFactorProperty, ScrollViewerZoomFactorChanged);

                    // Log bitmap load.
                    bitmapLoadLogger.Dispose();
                };
                GenericMediaDataElement.VideoElement.Detailed = true;
            }

            //TODO: Create File Memory Cache system by copying https://github.com/Microsoft/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.UI/Cache/CacheBase.cs
            //TODO: General tips about file performance and property fetching: https://docs.microsoft.com/en-us/windows/uwp/files/fast-file-properties
            //TODO: Make transition to detailsPage smoother, step 1 try making it into a Modal, step 2 try this https://www.eternalcoding.com/?p=2723

            // Pick up on property changes inside of GenericMediaDataElement.
            /*GenericMediaDataElement.PropertyChanged += (sender, args) =>
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

            // If there is dominant color data.
            if (mediaData?.Meta?.AnnotationData?.ImagePropertiesAnnotation?.DominantColors?.Colors != null)
            {
                var dominantColor = mediaData.Meta.AnnotationData.ImagePropertiesAnnotation.DominantColors.Colors.OrderBy(ci => ci.PixelFraction).LastOrDefault();
                if (dominantColor != null && dominantColor.Color != null)
                {
                    // Turn Google Color into Windows Color.
                    var color = dominantColor.Color.GetColor();
                    var brightness = color.GetBrightness();
                    // If color is not bright.
                    if (brightness < 0.4f)
                    {
                        // Set page background to dominant color.
                        Background = new SolidColorBrush(dominantColor.Color.GetColor());
                    }
                }
            }

            // Allow back-navigation if all animations are null.
            if (imageAnimation == null && gifAnimation == null && videoAnimation == null)
                _animFinished = true;
        }

        private void ZoomToFit()
        {
            var vm = DataContext as DetailsPageViewModel;

            var needsToBeResized = (vm.MediaData.Meta.Width > MediaScrollViewer.ViewportWidth) || (vm.MediaData.Meta.Height > MediaScrollViewer.ViewportHeight);

            // Don't resize if not necessary.
            if (!needsToBeResized)
                return;

            var mediaAspect = MediaViewBox.ActualWidth / MediaViewBox.ActualHeight;
            var viewportAspect = MediaScrollViewer.ViewportWidth / MediaScrollViewer.ViewportHeight;

            if (mediaAspect > viewportAspect)
            {
                MediaViewBox.Width = MediaScrollViewer.ViewportWidth;

                // Calculate height by using width and aspect quotient.
                MediaViewBox.Height = MediaViewBox.Width / mediaAspect;

                MediaViewBox.StretchDirection = StretchDirection.Both;
            }

            if (viewportAspect > mediaAspect)
            {
                MediaViewBox.Height = MediaScrollViewer.ViewportHeight;

                // Calculate width by using height and aspect quotient.
                MediaViewBox.Width = MediaViewBox.Height * mediaAspect;

                MediaViewBox.StretchDirection = StretchDirection.Both;
            }
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
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsImage2", GenericMediaDataElement.ImageElement);
            if (_gif)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsGif2", GenericMediaDataElement.ImageElement);
            if (_video)
            {
                GenericMediaDataElement.VideoElement.Detailed = false;
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailsVideo2", GenericMediaDataElement.VideoElement);
            }
        }

        private void MediaScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ZoomToFit();
        }

        private void GenericMediaDataElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Don't allow user to pan if no zoom has been applied.
            if (MediaScrollViewer.ZoomFactor == 1.0f)
                return;

            MediaTransform.TranslateX += e.Delta.Translation.X / MediaScrollViewer.ZoomFactor;
            MediaTransform.TranslateY += e.Delta.Translation.Y / MediaScrollViewer.ZoomFactor;

            var elementBounds = MediaTransform.TransformBounds(new Rect(0.0, 0.0, GenericMediaDataElement.ActualWidth, GenericMediaDataElement.ActualHeight));
            var containerRect = new Rect(0.0, 0.0, MediaScrollViewer.ViewportWidth, MediaScrollViewer.ViewportHeight);
            var isPartiallyVisible = containerRect.Contains(new Point(elementBounds.Left, elementBounds.Top)) || containerRect.Contains(new Point(elementBounds.Right, elementBounds.Bottom));
            var isFullyVisible = containerRect.Contains(new Point(elementBounds.Left, elementBounds.Top)) && containerRect.Contains(new Point(elementBounds.Right, elementBounds.Bottom));

            // Check if the rectangle is completely in the window.
            // If it is not and intertia is occuring, stop the manipulation.
            if (e.IsInertial && !isPartiallyVisible)
            {
                e.Complete();
            }
        }

        private void ScrollViewerZoomFactorChanged(DependencyObject obj, DependencyProperty property)
        {
            var zoomFactor = (obj as ScrollViewer)?.ZoomFactor ?? 1.0f;
            // If no zoom is being applied.
            if (zoomFactor == 1.0f)
            {
                // Reset panning.
                /*MediaTransform.TranslateX = 0.0f;
                MediaTransform.TranslateY = 0.0f;*/
                var storyboard = new Storyboard();

                var translateYAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
                };

                var translateXAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
                };

                Storyboard.SetTarget(translateYAnimation, MediaTransform);
                Storyboard.SetTarget(translateXAnimation, MediaTransform);
                Storyboard.SetTargetProperty(translateYAnimation, "TranslateY");
                Storyboard.SetTargetProperty(translateXAnimation, "TranslateX");

                storyboard.Children.Add(translateYAnimation);
                storyboard.Children.Add(translateXAnimation);

                storyboard.Begin();

                // Don't allow further panning.
                return;
            }
        }
    }
}
