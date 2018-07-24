using DMO.Extensions;
using DMO.Models;
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

        public bool IsVideo { get; set; }

        public bool IsImage => !IsVideo;

        public MediaData VideoMediaData
        {
            get => _videoMediaData;
            set
            {
                _videoMediaData = value;
                MediaData = value;
            }
        }

        public MediaData ImageMediaData
        {
            get => _imageMediaData;
            set
            {
                _imageMediaData = value;
                MediaData = value;
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
                    return BytesToString((long)MediaData.BasicProperties.Size);
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
                    return MediaData.Meta.AnnotationData.FullTextAnnotation.Text;
                return "--";
            }
        }

        public List<WebEntity> Entities
        {
            get
            {
                var entities = new List<WebEntity>();
                if (MediaData != null && MediaData.Meta != null && MediaData.Meta.AnnotationData != null)
                {
                    entities = MediaData.Meta.AnnotationData.WebDetection.WebEntities;
                }
                return entities;
            }
        }

        #endregion

        #region Commands

        private DelegateCommand _infoCommand;
        private MediaData _videoMediaData;
        private MediaData _imageMediaData;

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

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return $"0 {suf[0]}";
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            //var byteCountSign = Math.Sign(byteCount);
            return $"{(/*byteCountSign * */num).ToString(CultureInfo.InstalledUICulture)} {suf[place]}";
        }

        #endregion
    }
}
