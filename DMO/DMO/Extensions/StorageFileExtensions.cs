using DMO.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DMO.Extensions
{
    public static class StorageFileExtensions
    {
        public static bool IsVideo(this StorageFile file)
        {
            var extension = file.FileType;
            if (extension.StartsWith('.'))
                extension.Remove(0, 1);
            var mimeType = MimeTypeMap.GetMimeType(extension);
            return mimeType.Split('/').FirstOrDefault().Equals("video", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
