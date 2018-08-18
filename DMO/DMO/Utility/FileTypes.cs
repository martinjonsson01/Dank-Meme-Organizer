using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class FileTypes
    {
        public static List<string> Extensions
        {
            get
            {
                var extensions = new List<string>();
                foreach(var mimeType in MIMETypes)
                {
                    var mimeExtensions = MimeTypeMap.GetExtensionsOfMimeType(mimeType);
                    extensions.AddRange(mimeExtensions);
                }
                return extensions;
            }
        }
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

        public static bool IsSupportedExtension(string extension)
        {
            if (extension.StartsWith('.'))
                extension.Remove(0, 1);
            var mimeType = MimeTypeMap.GetMimeType(extension);
            return MIMETypes.Contains(mimeType.Split('/').FirstOrDefault().ToLowerInvariant());
        }
    }
}
