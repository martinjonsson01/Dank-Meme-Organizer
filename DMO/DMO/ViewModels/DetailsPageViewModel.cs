using DMO.Extensions;
using DMO.Models;
using DMO.Utility;
using DMO_Model.GoogleAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class DetailsPageViewModel : ViewModelBase
    {
        #region Public Properties

        public string MediaDataKey { get; set; }

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
                    foreach(var entity in MediaData.Meta.AnnotationData.WebDetection.WebEntities)
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
                    foreach(var image in MediaData.Meta.AnnotationData.WebDetection.FullMatchingImages)
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

        #endregion

        #region Commands

        private DelegateCommand _infoCommand;
        public DelegateCommand InfoCommand
            => _infoCommand ?? (_infoCommand = new DelegateCommand(() =>
            {
                InfoOpen = !InfoOpen;
            }));

        #endregion

        #region Constructor

        public DetailsPageViewModel()
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
        
        #endregion
    }
}
