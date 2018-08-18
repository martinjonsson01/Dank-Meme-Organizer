using DMO.Extensions;
using DMO.Models;
using DMO.Utility;
using DMO_Model.GoogleAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class AlternateDetailsPageViewModel : ViewModelBase
    {

        #region Private Members

        #endregion

        #region Public Properties

        public string MediaDataKey { get; set; }

        public string MediaSource
        {
            get
            {
                return MediaData?.Meta?.MediaFilePath ?? MediaData?.MediaFile?.Path;
            }
        }
        
        public MediaData MediaData { get; set; }

        public bool InfoOpen { get; set; }

        public string Filename
        {
            get
            {
                if (MediaData != null)
                    return MediaData.Title;
                return "--";
            }
            set
            {
                if (MediaData != null)
                    MediaData.Title = value;
            }
        }

        public string FolderPath => Path.GetDirectoryName(MediaData?.MediaFile?.Path) ?? "--";

        public string Created
        {
            get
            {
                if (MediaData != null)
                    return MediaData.Created.ToString("f", CultureInfo.InstalledUICulture);
                return "--";
            }
        }

        public string Added
        {
            get
            {
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.DateAdded != null)
                    return MediaData.Meta.DateAdded.ToString("f", CultureInfo.InstalledUICulture);
                return "--";
            }
        }

        public string Size
        {
            get
            {
                if (MediaData != null && MediaData.BasicProperties != null)
                    return ((long)MediaData.BasicProperties.Size).BytesToString();
                return "--";
            }
        }

        public string Dimensions
        {
            get
            {
                if (MediaData != null && MediaData.Meta != null)
                    return $"{MediaData.Meta.Width} x {MediaData.Meta.Height}";
                return "--";
            }
        }

        public string Text
        {
            get
            {
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.AnnotationData != null)
                    return MediaData.Meta.AnnotationData?.FullTextAnnotation?.Text ?? "No text";
                return "--";
            }
        }

        public List<WebEntity> Entities
        {
            get
            {
                var entities = new List<WebEntity>();
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.AnnotationData != null && MediaData.Meta.AnnotationData.WebDetection != null)
                {
                    foreach (var entity in MediaData.Meta.AnnotationData.WebDetection.WebEntities)
                    {
                        // Ignore all entities with no descriptions.
                        if (string.IsNullOrEmpty(entity.Description)) continue;

                        entities.Add(entity);
                    }
                }
                return entities;
            }
        }

        public bool FullyMatchedImagesContainsItems => FullyMatchedImages.Count > 0;

        public List<WebImage> FullyMatchedImages
        {
            get
            {
                var images = new List<WebImage>();
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.AnnotationData != null && MediaData.Meta.AnnotationData.WebDetection != null)
                {
                    foreach (var image in MediaData.Meta.AnnotationData.WebDetection.FullMatchingImages)
                    {
                        if (Uri.TryCreate(image.Url, UriKind.Absolute, out var uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            images.Add(image);
                        }
                    }
                }
                return images;
            }
        }

        public bool PartiallyMatchedImagesContainsItems => PartiallyMatchedImages.Count > 0;

        public List<WebImage> PartiallyMatchedImages
        {
            get
            {
                var images = new List<WebImage>();
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.AnnotationData != null && MediaData.Meta.AnnotationData.WebDetection != null)
                {
                    foreach (var image in MediaData.Meta.AnnotationData.WebDetection.PartialMatchingImages)
                    {
                        if (Uri.TryCreate(image.Url, UriKind.Absolute, out var uriResult)
                            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                        {
                            images.Add(image);
                        }
                    }
                }
                return images;
            }
        }

        public Uri LargerMedia { get; set; }

        public bool IsDownloadingLarger { get; set; }

        #endregion

        #region Commands

        private DelegateCommand _infoCommand;
        public DelegateCommand InfoCommand
            => _infoCommand ?? (_infoCommand = new DelegateCommand(() =>
            {
                InfoOpen = !InfoOpen;
            }));

        private DelegateCommand _openFolderCommand;

        public DelegateCommand OpenFolderCommand
            => _openFolderCommand ?? (_openFolderCommand = new DelegateCommand(async () =>
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);
                    if (folder == null) return;
                    var options = new FolderLauncherOptions
                    {
                    DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseMore
                    };
                    if (MediaData.MediaFile != null)
                        options.ItemsToSelect.Add(MediaData.MediaFile);

                    await Launcher.LaunchFolderAsync(folder, options);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Could not open folder in explorer due to exception:");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }));

        private DelegateCommand _downloadAndCompareCommand;
        public DelegateCommand DownloadAndCompareCommand
            => _downloadAndCompareCommand ?? (_downloadAndCompareCommand = new DelegateCommand(async () =>
            {
                // Get gallery folder.
                var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");
                // Download media.
                IsDownloadingLarger = true;
                var newMediaFile = await OnlineUtil.DownloadFileAsync(LargerMedia, folder);
                IsDownloadingLarger = false;
                // Go back to show the newly downloaded file.
                NavigationService.GoBack();
                // Open duplication screen. TODO: This should happen automatically. Maybe add a function to manually trigger a check?
            }));

        #endregion

        #region Constructor

        public AlternateDetailsPageViewModel()
        {

        }

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(MediaDataKey)] = MediaDataKey;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        public async void SearchForHigherResolutionOnlineAsync()
        {
            // Don't scan for videos.
            if (MediaData is VideoData) return;

            var onlineMedias = new Dictionary<Uri, (Size, long)>();

            // Get file size and dimensions of every fully matched image.
            foreach (var webImage in FullyMatchedImages)
            {
                // Parse URL.
                if (Uri.TryCreate(webImage.Url, UriKind.Absolute, out var uri))
                {
                    // Check to make sure this URL points to a file and not a webpage.
                    if (Path.HasExtension(uri.AbsoluteUri))
                    {
                        // Get MIME type of online file.
                        var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(uri.AbsoluteUri));
                        // Check if MIME type of online file is supported.
                        if (FileTypes.IsSupportedMIME(mimeType))
                        {
                            try
                            {
                                // Get dimensions of online image.
                                var dimensions = await ImageUtilities.GetWebDimensionsAsync(uri);
                                // If dimensions is empty, continue.
                                if (dimensions.IsEmpty) continue;
                                // Get file size of online image.
                                var contentLength = await OnlineUtil.GetContentSizeAsync(uri);

                                // Add to dictionary.
                                onlineMedias.Add(uri, (dimensions, contentLength));
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("Exception occured when finding size and dimensions of online media:");
                                Debug.WriteLine(e.Message);
                                Debug.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }
            }

            // If these are null then no comparisons can be made.
            if (MediaData?.Meta == null) return;
            if (MediaData?.BasicProperties == null) return;

            // Check if any of the fully matched images are higher resolution or larger than local file.
            var largerMedia = new KeyValuePair<Uri, (Size, long)>();
            foreach(var urlAndData in onlineMedias)
            {
                var url = urlAndData.Key;
                var (dimensions, contentLength) = urlAndData.Value;

                // If online media is larger in file size than local media.
                if (contentLength > (long)MediaData.BasicProperties.Size)
                {
                    // If size of this online media is larger than the currently largest media.
                    if (Math.Max(largerMedia.Value.Item2, contentLength) == contentLength)
                        largerMedia = urlAndData; // Update largerMedia.
                }
                else if (dimensions.Height > MediaData.Meta.Height && // If online media is larger in both width and height
                        dimensions.Width > MediaData.Meta.Width)      // than the local media.
                {
                    // If dimensions of this online media is larger than the currently largest media.
                    if (Math.Max(largerMedia.Value.Item1.Height, dimensions.Height) == dimensions.Height &&
                        Math.Max(largerMedia.Value.Item1.Width, dimensions.Width) == dimensions.Width)
                        largerMedia = urlAndData; // Update largerMedia.
                }
            }

            // If no larger media has been found, return.
            if (largerMedia.Key == null) return;

            // Set property.
            LargerMedia = largerMedia.Key;
        }

        #endregion
    }
}
