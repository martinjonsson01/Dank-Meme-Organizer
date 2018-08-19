using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DMO.Utility.Logging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace DMO.Utility
{
    public static class OnlineUtil
    {
        public static async Task<long> GetContentSizeAsync(Uri uri)
        {
            // Time and log getting of online file size.
            using (new DisposableLogger(WebLog.GetOnlineFileSizeBegin, WebLog.GetOnlineFileSizeEnd))
            {
                var req = WebRequest.Create("http://example.com/") as HttpWebRequest;
                req.Method = "HEAD";
                using (var resp = await req.GetResponseAsync() as HttpWebResponse)
                {
                    return resp.ContentLength;
                }
            }
        }

        /// <summary>
        /// Downloads a file.
        /// Note: Returns before download is complete.
        /// </summary>
        /// <param name="uri">The web-address to download the file from.</param>
        /// <param name="folder">The folder to download the file to.</param>
        public static async void DownloadFile(Uri uri, StorageFolder folder)
        {
            await DownloadFileAsync(uri, folder);
        }

        /// <summary>
        /// Downloads a file asynchronously.
        /// </summary>
        /// <param name="uri">The web-address to download the file from.</param>
        /// <param name="folder">The folder to download the file to.</param>
        public static async Task<StorageFile> DownloadFileAsync(Uri uri, StorageFolder folder)
        {
            // Create download file path.
            var fileName = uri.Segments.Last();
            var downloadFilePath = Path.Combine(folder.Path, fileName);
            // Create download file.
            var downloadFile = await folder.CreateFileAsync(uri.Segments.Last(), CreationCollisionOption.GenerateUniqueName);
            // Download file.
            var downloader = new BackgroundDownloader();
            var downloadOperation = downloader.CreateDownload(uri, downloadFile);
            // Wait for download to complete.
            await downloadOperation.StartAsync();
            // Return the downloaded file.
            return downloadFile;
            // Add to gallery. TODO: Is this needed? Maybe revamp it so it doesn't do things twice.
            //await AddFileToGalleryAsync(vm, downloadFile, folder);
        }

    }
}
