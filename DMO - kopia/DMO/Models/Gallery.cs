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
using System.Linq;

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

        public bool IsEvaluating { get; set; }

        #endregion

        #region Constructor

        public Gallery(string rootFolderPath)
        {
            RootFolderPath = rootFolderPath;
            MediaDatas = new ObservableCollection<MediaData>();
            App.Gallery = this;
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

        /// <summary>
        /// Adds a new <see cref="StorageFile"/> to this gallery.
        /// </summary>
        /// <param name="imageSize">The size of the pregenerated thumbnail.</param>
        /// <param name="mediaFile">The file to add.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes a <see cref="StorageFile"/> from this gallery.
        /// Note: Does not remove the file from the file system.
        /// </summary>
        /// <param name="mediaFile">The file to remove.</param>
        public void RemoveFile(StorageFile mediaFile)
        {
            var firstMatch = MediaDatas.ToList().First(mediaData => mediaData.MediaFile == mediaFile);
            var index = MediaDatas.IndexOf(firstMatch);
            MediaDatas.RemoveAt(index);
            App.Files.Remove(mediaFile.Name);
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
            var sw = new Stopwatch();
            sw.Start();
            IsEvaluating = true;
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
                    // Evaluate image using classifier if it has no tags.
                    if (image.Tags.Count < 1)
                        await image.Evaluate(MemeClassifier);
                    // Update and then report progress.
                    evaluated++;
                    progress.Report(evaluated);
                }
            }
            IsEvaluating = false;
            sw.Stop();
            Debug.WriteLine($"Gallery Machine Learning Evaluation completed! Elapsed time: {sw.ElapsedMilliseconds} ms {evaluated} files evaluated." +
                $" Average time per file {sw.ElapsedMilliseconds/(float)evaluated} ms");
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
