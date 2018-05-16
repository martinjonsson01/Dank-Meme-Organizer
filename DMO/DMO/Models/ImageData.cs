using System;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class ImageData : MediaData
    {
        public ImageData(StorageFile file) : base(file)
        {

        }
    }
}
