using DMO.GoogleAPI;
using DMO.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class VideoData : MediaData
    {
        public bool Suspended { get; set; } = false;

        public VideoData(StorageFile file) : base(file)
        {

        }

        /// <summary>
        /// To be used by EntityFramework only.
        /// </summary>
        public VideoData() : base()
        {

        }

        public override async Task<IRandomAccessStream> GetThumbnailAsync()
        {
            var mediaClip = await MediaClip.CreateFromFileAsync(MediaFile);
            var mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(mediaClip);

            // Get thumbnail stream from frame in the middle of the video.
            var halfDuration = mediaComposition.Duration / 2;
            var thumbnailStream = await mediaComposition.GetThumbnailAsync(
                halfDuration, 0, 0, VideoFramePrecision.NearestKeyFrame);

            // Schedule the loading of the dimensions of the video on main thread when it is idle.
            await App.Current.NavigationService.Frame.Dispatcher.RunIdleAsync((args) => LoadDimensions(thumbnailStream));

            return thumbnailStream;
        }

        private async void LoadDimensions(IRandomAccessStream thumbnailStream)
        {
            // Turn stream into bitmap and get width and height.
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(thumbnailStream);
            // Get mediaData from gallery.
            var mediaData = App.Gallery.MediaDatas.FirstOrDefault(data => data.Meta.MediaFilePath.Equals(MediaFile.Path));
            if (mediaData != null)
            {
                // Update meta height and width.
                mediaData.Meta.Height = bitmap.PixelHeight;
                mediaData.Meta.Width = bitmap.PixelWidth;
            }
        }
    }
}
