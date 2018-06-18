using DMO.Extensions;
using DMO.ML;
using DMO.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class Gallery : BaseModel
    {
        #region Private Members

        private MemeClassifierModel MemeClassifier;

        #endregion

        #region Public Properties

        public string RootFolderPath { get; }

        public ObservableCollection<MediaData> MediaDatas { get; }

        public int FilesFound { get; set; }

        public int ImageCount { get; private set; }
        public int VideoCount { get; private set; }
        public int GifCount { get; private set; }

        #endregion

        #region Constructor

        public Gallery(string rootFolderPath)
        {
            RootFolderPath = rootFolderPath;
            MediaDatas = new ObservableCollection<MediaData>();
        }

        #endregion

        #region Public Methods

        public async Task LoadFolderContents(int imageSize)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(RootFolderPath);

            if (folder != null)
            {
                var sw = new Stopwatch();
                sw.Start();
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, FileTypes.Extensions)
                {
                    FolderDepth = FolderDepth.Deep,
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                };

                // Prefetch thumbnails.
                queryOptions.SetThumbnailPrefetch(ThumbnailMode.SingleItem, (uint)imageSize, ThumbnailOptions.ResizeThumbnail);
                queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.ImageProperties, new[] { "System.DateModified" });

                // Create query.
                var query = folder.CreateFileQueryWithOptions(queryOptions);

                var mediaFiles = await query.GetFilesAsync();
                FilesFound = mediaFiles.Count;
                sw.Stop();
                Debug.WriteLine($"File query completed! Elapsed time: {sw.ElapsedMilliseconds} ms {mediaFiles.Count} files found.");

                sw.Reset();
                sw.Start();
                foreach (var mediaFile in mediaFiles)
                {
                    if (FileTypes.IsSupportedMIME(mediaFile.ContentType))
                        await AddFile(imageSize, mediaFile);
                }
                sw.Stop();
                Debug.WriteLine($"File parsing completed! Elapsed time: {sw.ElapsedMilliseconds} ms {mediaFiles.Count} files parsed.");
            }
        }

        public async Task AddFile(int imageSize, StorageFile mediaFile)
        {
            var data = CreateMediaData(mediaFile);

            var properties = await mediaFile.Properties.RetrievePropertiesAsync
                (
                    new String[] { "System.DateModified" }
                );
            var lastModified = properties["System.DateModified"];
            if (lastModified is DateTime lastModifiedDateTime)
            {
                data.LastModified = lastModifiedDateTime;
            }
            MediaDatas.Add(data);

            if (!App.Files.ContainsKey(mediaFile.Name))
                App.Files.Add(mediaFile.Name, mediaFile);

            await ApplyThumbnails(imageSize, mediaFile, data);
        }

        public static async Task ApplyThumbnails(int imageSize, StorageFile mediaFile, MediaData data)
        {
            //
            // Apply bitmap source depending on file type.
            //
            switch (mediaFile.FileType)
            {
                case ".gif":
                    using (var gifStream = await mediaFile.OpenReadAsync())
                    {
                        // Can't use thumbnail for gifs since they are animated, so use a data stream instead.
                        //await data.Thumbnail.SetSourceAsync(gifStream);
                    }
                    break;
                default:
                    // Get the thumbnail of this file from windows (is fast since windows caches them).
                    var thumbnail = await mediaFile.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)imageSize, ThumbnailOptions.ResizeThumbnail);

                    // Thumbnails for everything but gifs do not need to be animated so a static thumbnail is fine.
                    //await data.Thumbnail.SetSourceAsync(thumbnail);
                    break;
            }
        }

        public MediaData CreateMediaData(StorageFile mediaFile)
        {
            //
            // Instantiate MediaData type depending on FileType.
            //
            if (mediaFile.FileType == ".gif")
            {
                GifCount++;
                return new GifData(mediaFile);
            }
            else if (mediaFile.IsVideo())
            {
                // Generate deterministic UID for file.
                /*var uid = new Uri(mediaFile.Path).AbsolutePath;
                // If uid is over character limit shorten it from the right side so it keeps its uniqueness.
                if (uid.Length >= 260)
                    uid = uid.Substring(uid.Length - 261, uid.Length - 1);
                // Store file for future access.
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(uid, mediaFile);*/
                VideoCount++;
                return new VideoData(mediaFile);
            }
            else
            {
                ImageCount++;
                return new ImageData(mediaFile);
            }
        }

        public async Task EvaluateImages(IProgress<int> progress)
        {
            // Load ML model into memory if not already.
            if (MemeClassifier == null)
            {
                var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/MemeClassifier.onnx"));
                MemeClassifier = await MemeClassifierModel.CreateModel(modelFile);
            }

            var evaluated = 0;
            foreach (var data in MediaDatas)
            {
                if (data is ImageData image)
                {
                    // Evaluate image using classifier.
                    await image.Evaluate(MemeClassifier);
                    // Update and then report progress.
                    evaluated++;
                    progress.Report(evaluated);
                }
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
