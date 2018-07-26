using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class StringUtils
    {
        public static string EllipsisString(this string rawString, int maxLength = 30, string delimiter = @"/")
        {
            maxLength -= 3; //account for delimiter spacing

            if (rawString.Length <= maxLength)
            {
                return rawString;
            }

            string final = rawString;
            var uri = new Uri(rawString, UriKind.Absolute);

            if (uri.Scheme == "file")
                return rawString.EllipsisString1(maxLength + 3, "\\");

            var scheme = $"{uri.Scheme}:/";
            var host = uri.Host;
            var filename = uri.Segments.Last();

            if (scheme.Length + host.Length + 3 + filename.Length < maxLength)
                return string.Join(delimiter, new[] { scheme, host, "...", filename });
            if (host.Length + 3 + filename.Length < maxLength)
                return string.Join(delimiter, new[] { host, "...", filename });
            if (scheme.Length + 3 + filename.Length < maxLength)
                return string.Join(delimiter, new[] { scheme, "...", filename });
            else
                return filename.Truncate(maxLength);
        }

        public static string EllipsisString1(this string rawString, int maxLength = 30, string delimiter = @"/")
        {
            maxLength -= 3; //account for delimiter spacing

            if (rawString.Length <= maxLength)
            {
                return rawString;
            }

            string final = rawString;
            List<string> parts;

            int loops = 0;
            while (loops++ < 100)
            {
                parts = rawString.Split(delimiter).ToList();
                parts.RemoveRange(parts.Count - 1 - loops, loops);
                if (parts.Count == 1)
                {
                    return parts.Last().Truncate(maxLength);
                }

                parts.Insert(parts.Count - 1, "...");
                final = string.Join(delimiter.ToString(), parts);
                if (final.Length < maxLength)
                {
                    return final;
                }
            }

            return rawString.Split(delimiter).ToList().Last().Truncate(maxLength);
        }

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : "..." + value.Substring(value.Length - maxChars);
        }

        public static string BytesToString(this long byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return $"0 {suf[0]}";
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            //var byteCountSign = Math.Sign(byteCount);
            return $"{(/*byteCountSign * */num).ToString(CultureInfo.InstalledUICulture)} {suf[place]}";
        }
    }
}
