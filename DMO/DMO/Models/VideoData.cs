using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class VideoData : MediaData
    {
        public bool Suspended { get; set; } = false;

        public VideoData(StorageFile file) : base(file)
        {

        }
    }
}
