using DMO.Database;
using DMO.Extensions;
using DMO.GoogleAPI;
using DMO.ML;
using DMO.Services.SettingsServices;
using DMO.Utility;
using DMO_Model.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        private MemeClassifierModel _memeClassifier;

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

        public Gallery()
        {
            RootFolderPath = SettingsService.Instance.FolderPath;
            MediaDatas = new ObservableCollection<MediaData>();
            App.Gallery = this;
        }

        #endregion

        #region Public Methods

        public async Task LoadMediaFiles(ICollection<MediaData> mediaDatas, int imageSize)
        {
            var sw = new Stopwatch();
            sw.Start();
            FilesFound = mediaDatas.Count;
            foreach (var mediaData in mediaDatas)
            {
                if (mediaData is null) continue;
                if (mediaData.Meta is null) continue;
                if (string.IsNullOrEmpty(mediaData.Meta.MediaFilePath)) continue;

                var mediaFile = await StorageFile.GetFileFromPathAsync(mediaData.Meta.MediaFilePath);

                if (mediaFile is null) continue;

                // Tell the mediaData which file belongs to it.
                mediaData.MediaFile = mediaFile;

                if (FileTypes.IsSupportedMIME(mediaFile.ContentType))
                    await AddFile(imageSize, mediaFile, mediaData);
            }
            sw.Stop();
            Debug.WriteLine($"File parsing completed! Elapsed time: {sw.ElapsedMilliseconds} ms {mediaDatas.Count} files parsed.");
        }

        public async Task LoadFolderContents(int imageSize)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem("gallery"))
                return;

            var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");

            // TODO: Set up a tracker to keep track of when new files get added to the file system while application is running.
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
        /// <param name="mediaData">Optional, the data of the file to add.</param>
        /// <returns></returns>
        public async Task AddFile(int imageSize, StorageFile mediaFile, MediaData data = null)
        {
            if (data == null)
                data = MediaData.CreateFromStorageFile(mediaFile);

            if (data is GifData)
                GifCount++;
            if (data is VideoData)
                VideoCount++;
            if (data is ImageData)
                ImageCount++;

            var imageProp = await mediaFile.Properties.GetImagePropertiesAsync();
            data.Meta.Height = (int)imageProp.Height;
            data.Meta.Width = (int)imageProp.Width;
            data.BasicProperties = await mediaFile.GetBasicPropertiesAsync();
            var properties = await mediaFile.Properties.RetrievePropertiesAsync
                (
                    new String[] { "System.DateModified" }
                );
            var lastModified = properties["System.DateModified"];
            if (lastModified is DateTime lastModifiedDateTime)
            {
                data.Meta.LastModified = lastModifiedDateTime;
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

        public async Task EvaluateImagesOnline(IProgress<int> progress, CancellationToken cancellationToken)
        {
            // Sign into firebase.
            if (await FirebaseClient.Client.SignInPromptUserIfNecessary())
            {
                // Find all media datas without annotation data.
                var mediaDatasWithoutAnnotationData = MediaDatas.Where(data => data.Meta.AnnotationData == null);

                var sw = new Stopwatch();
                sw.Start();
                IsEvaluating = true;
                var evaluated = 0;
                await TaskUtils.ForEachAsyncConcurrent(mediaDatasWithoutAnnotationData, async media =>
                {
                    // Evaluate media thumbnail online.
                    await media.EvaluateOnlineAsync(FirebaseClient.accessToken);
                    // Update and then report progress.
                    evaluated++;
                    progress.Report(evaluated);
                }, cancellationToken, 5);
                IsEvaluating = false;
                sw.Stop();
                Debug.WriteLine($"Gallery Cloud Vision Evaluation completed! Elapsed time: {sw.ElapsedMilliseconds} ms {evaluated} files evaluated." +
                    $" Average time per file {sw.ElapsedMilliseconds / (float)evaluated} ms");

                // Save online results to database.
                using (var context = new MediaMetaDatabaseContext())
                {
                    await context.SaveAllMetadatasAsync(MediaDatas);
                }
            }
        }

        public async Task EvaluateImagesLocally(IProgress<int> progress, CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            sw.Start();
            IsEvaluating = true;
            
            await LoadLocalModel();

            var evaluated = 0;
            foreach (var data in MediaDatas)
            {
                // Throw if cancellation is requested.
                cancellationToken.ThrowIfCancellationRequested();

                // Evaluate media using classifier if it has no tags already.
                if (data.Meta?.Labels == null || data.Meta?.Labels.Count < 1)
                    await data.EvaluateLocalAsync(_memeClassifier);
                // Update and then report progress.
                evaluated++;
                progress.Report(evaluated);
            }
            IsEvaluating = false;
            sw.Stop();
            Debug.WriteLine($"Gallery Machine Learning Evaluation completed! Elapsed time: {sw.ElapsedMilliseconds} ms {evaluated} files evaluated." +
                $" Average time per file {sw.ElapsedMilliseconds / (float)evaluated} ms");

            // Don't save local results to database since they will be saved together with the online results shortly.
        }

        public async Task LoadLocalModel()
        {
            // Load ML model into memory if not already.
            if (_memeClassifier == null)
            {
                var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/MemeClassifier.onnx"));
                _memeClassifier = await MemeClassifierModel.CreateModel(modelFile);
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
