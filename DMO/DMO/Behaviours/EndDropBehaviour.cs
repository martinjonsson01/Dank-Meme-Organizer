using DMO.Models;
using DMO.Utility;
using DMO.Utility.Logging;
using DMO.ViewModels;
using Microsoft.Xaml.Interactivity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace DMO.Behaviours
{
    public class EndDropBehavior : Behavior<GridView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
        }

        private async void AssociatedObject_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView != null &&
                e.DataView.Properties != null)
            {
                var def = e.GetDeferral();
                
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    if (sender is GridView grid)
                    {
                        if (grid.DataContext is GalleryPageViewModel vm)
                        {
                            var items = await e.DataView.GetStorageItemsAsync();
                            foreach (var item in items)
                            {
                                if (item is StorageFile file)
                                {
                                    if (FileTypes.IsSupportedExtension(file.FileType)) // Check if file type is supported. Using MIME allows for all kinds of videos and images.
                                        await CopyLocalFile(vm, file);
                                    else if (file.FileType.Equals(".url", StringComparison.InvariantCultureIgnoreCase)) // If file is an internet shortcut.
                                        await ParseUrlFile(file);
                                }
                            }
                        }
                    }
                }

                def.Complete();
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        
        private static async Task CopyLocalFile(GalleryPageViewModel vm, StorageFile file)
        {
            var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");
            await AddFileToGalleryAsync(vm, file, folder);
        }

        private static async Task ParseUrlFile(StorageFile file)
        {
            // Try to read .url file.
            string url = await ParseInternetShortcut(file);

            // Parse URL.
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) // This is true if file is online.
            {
                switch (uri.Host)
                {
                    case "www.reddit.com":
                        await DownloadFromRedditUriAsync(uri);
                        break;

                    default:
                        await DownloadFromUriAsync(uri);
                        break;
                }
            }
        }

        private static async Task DownloadFromRedditUriAsync(Uri redditUri)
        {
            // Get JSON version of reddit page.
            var json = await App.HttpClient.GetStringAsync(redditUri.ToString() + "/.json");

            var jsonDatas = JArray.Parse(json);
            var jsonData = jsonDatas.FirstOrDefault();
            //string imageUrl = jsonData?.data?.children[0]?.data?.url;
            var imageUrl = jsonData?.Value<JToken>("data")?.Value<JArray>("children")?.FirstOrDefault()?.Value<JToken>("data")?.Value<string>("url") ?? string.Empty;
            if (string.IsNullOrEmpty(imageUrl))
                return;

            // Parse URL.
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri) &&
                (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps)) // This is true if file is online.
            {
                // Check to make sure this URL points to a file and not a webpage.
                if (Path.HasExtension(imageUri.AbsoluteUri))
                {
                    await DownloadFromUriAsync(imageUri);
                }
                else // It might be a video, check if it is.
                {
                    string videoUrl = jsonData?.Value<JToken>("data")?.Value<JArray>("children")?.FirstOrDefault()?.Value<JToken>("data")?.Value<JToken>("media")?.Value<JToken>("reddit_video")?.Value<string>("fallback_url") ?? string.Empty;
                    if (string.IsNullOrEmpty(videoUrl))
                        return;

                    // Parse URL.
                    if (Uri.TryCreate(videoUrl, UriKind.Absolute, out var videoUri) &&
                        (videoUri.Scheme == Uri.UriSchemeHttp || videoUri.Scheme == Uri.UriSchemeHttps)) // This is true if file is online.
                    {
                        var urlContentType = await GetUrlContentTypeAsync(videoUrl);
                        // Check if url points to video before downloading.
                        if (urlContentType.StartsWith("video/"))
                        {
                            // Get folder.
                            var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");
                            // Append extension to uri.
                            var fileName = videoUri.Segments[videoUri.Segments.Length - 1] + $".{urlContentType.Split("/").LastOrDefault() ?? "mp4"}";
                            // Download without awaiting so the UI doesn't freeze.
                            OnlineUtil.DownloadFile(videoUri, fileName, folder);
                        }
                    }
                }
            }
        }

        private static async Task DownloadFromUriAsync(Uri uri)
        {
            // Check to make sure this URL points to a file and not a webpage.
            if (Path.HasExtension(uri.AbsoluteUri))
            {
                // Get MIME type of online file.
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(uri.AbsoluteUri));
                // Check if MIME type of online file is supported.
                if (FileTypes.IsSupportedMIME(mimeType))
                {
                    // Get folder.
                    var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");
                    // Download without awaiting so the UI doesn't freeze.
                    OnlineUtil.DownloadFile(uri, folder);
                }
            }
        }

        private void AssociatedObject_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                // Specify allowed operation.   
                e.AcceptedOperation = DataPackageOperation.Copy;

                // To show the user some information.
                e.DragUIOverride.Caption = "Drop Here to Add to Gallery";
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Drop -= AssociatedObject_Drop;

            AssociatedObject.DragOver -= AssociatedObject_DragOver;
        }

        private static async Task<string> ParseInternetShortcut(StorageFile file)
        {
            try
            {
                using(var stream = await file.OpenStreamForReadAsync())
                {
                    var lines = ReadLines(stream, Encoding.UTF8);
                    string line = lines.Skip(1).Take(1).First();
                    string url = line.Replace("URL=", "");
                    url = url.Replace("\"", "");
                    url = url.Replace("BASE", "");
                    return url;
                }
            }
            catch (Exception e)
            {
                // Log Exception.
                LifecycleLog.Exception(e);
                return string.Empty;
            }
        }

        public static IEnumerable<string> ReadLines(Stream stream,
                                     Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static async Task AddFileToGalleryAsync(GalleryPageViewModel vm, StorageFile file, StorageFolder folder)
        {
            var mediaFile = await file.CopyAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName);
            // Add new media file to gallery.
            //await vm.Gallery.AddFileAsync(vm.TileSize, mediaFile);
        }

        private static async Task<string> GetUrlContentTypeAsync(string URL)
        {
            var req = (HttpWebRequest)WebRequest.Create(URL);
            req.Method = "HEAD";
            using (var resp = await req.GetResponseAsync())
            {
                return resp.ContentType.ToLower(CultureInfo.InvariantCulture);
            }
        }
    }
}
