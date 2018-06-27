using System;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class GifData : MediaData
    {
        public GifData(StorageFile file) : base(file)
        {

        }
    }
}
