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
            switch (file.FileType)
            {
                case ".mp4":
                case ".mov":
                case ".webm":
                    return true;
                default:
                    return false;
            }
        }
    }
}
