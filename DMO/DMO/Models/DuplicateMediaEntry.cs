using DMO.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Models
{
    public class DuplicateMediaEntry : BaseModel
    {
        public MediaData MediaData { get; set; }

        public string Path => MediaData?.MediaFile?.Path ?? "--";

        public string Size => ((long)MediaData?.BasicProperties?.Size).BytesToString() ?? "--";

        public string Dimensions => $"{MediaData?.Meta?.Width} x {MediaData?.Meta?.Height}";

        public string Added => MediaData?.Meta?.DateAdded.ToString("f", CultureInfo.InstalledUICulture) ?? "--";

        public DuplicateMediaEntry(MediaData mediaData)
        {
            MediaData = mediaData;
        }
    }
}
