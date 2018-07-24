using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class WebUtils
    {
        public async static Task<bool> IsImageUrl(string URL)
        {
            var req = WebRequest.Create(URL);
            req.Method = "HEAD";
            using (var resp = await req.GetResponseAsync())
            {
                return resp.ContentType.ToLowerInvariant()
                           .StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
