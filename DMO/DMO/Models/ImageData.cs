using DMO.GoogleAPI;
using DMO.ML;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DMO.Models
{
    public class ImageData : MediaData
    {
        public ImageData(StorageFile file) : base(file)
        {

        }

        public override async Task<IRandomAccessStream> GetThumbnailAsync()
        {
            return await MediaFile.OpenAsync(FileAccessMode.Read);
        }
    }
}
