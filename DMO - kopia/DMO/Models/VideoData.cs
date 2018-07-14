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

        public override async Task<IRandomAccessStream> GetThumbnailAsync()
        {
            var mediaClip = await MediaClip.CreateFromFileAsync(MediaFile);
            var mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(mediaClip);
            return await mediaComposition.GetThumbnailAsync(
                TimeSpan.FromMilliseconds(0), 0, 0, VideoFramePrecision.NearestKeyFrame);
        }
    }
}
