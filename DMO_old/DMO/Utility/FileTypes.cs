using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class FileTypes
    {
        public static List<string> Extensions = new List<string>
        {
            ".jpg",
            ".png",
            ".gif",
            ".mp4",
            ".mov",
            ".webm",
        };
        public static List<string> MIMETypes = new List<string>
        {
            "image",
            "video",
        };

        public static bool IsSupportedMIME(string MIMEType)
        {
            var types = MIMEType.Split('/');
            return MIMETypes.Contains(types[0]);
        }
    }
}
