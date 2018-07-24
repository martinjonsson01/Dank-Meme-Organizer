using DMO.GoogleAPI;
using DMO.ML;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DMO.Models
{
    public class GifData : MediaData
    {
        public GifData(StorageFile file) : base(file)
        {

        }

        /// <summary>
        /// To be used by EntityFramework only.
        /// </summary>
        public GifData() : base()
        {

        }

        public override async Task<IRandomAccessStream> GetThumbnailAsync()
        {
            return await MediaFile.OpenAsync(FileAccessMode.Read);
        }
    }
}
