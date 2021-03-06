﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMO.GoogleAPI;
using DMO.ML;
using DMO.Services.SettingsServices;
using DMO.Utility;
using DMO.Utility.Logging;
using DMO.Views;
using DMO_Model.Models;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace DMO.Models
{
    public class Gallery : BaseModel
    {
        #region Private Members

        private MemeClassifierModel _memeClassifier;

        private StorageFileQueryResult _query;

        private StorageFolder _folder;

        /// <summary>
        /// This list contains media StorageFiles that have lost its MediaData.
        /// This is usually caused by the file being deleted, moved or renamed while
        /// the application was off.
        /// </summary>
        private List<StorageFile> _lostMediaFiles = new List<StorageFile>();

        #endregion

        #region Public Properties

        public string RootFolderPath { get; }

        public ObservableCollection<MediaData> MediaDatas { get; }

        public int ImageSize { get; set; }

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

        public async Task LoadFolderContents(int imageSize, IProgress<int> progress, ICollection<MediaData> mediaDatas = null)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem("gallery"))
                return;
            
            ImageSize = imageSize;

            _folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");

            if (_folder != null)
            {
                // Time and log file query.
                using (new DisposableLogger(GalleryLog.FileQueryBegin, (sw) => GalleryLog.FileQueryEnd(sw, FilesFound)))
                {
                    var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, FileTypes.Extensions)
                    {
                        FolderDepth = FolderDepth.Deep,
                        IndexerOption = IndexerOption.UseIndexerWhenAvailable,
                    };

                    // Sort results.
                    queryOptions.SortOrder.Clear();
                    var sortEntry = QueryUtils.GetSortEntryFromSettings();
                    queryOptions.SortOrder.Add(sortEntry);

                    // Prefetch thumbnails.
                    queryOptions.SetThumbnailPrefetch(ThumbnailMode.SingleItem, (uint)imageSize, ThumbnailOptions.UseCurrentScale);
                    queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.ImageProperties, new[] { "System.DateModified" });

                    // Create query.
                    _query = _folder.CreateFileQueryWithOptions(queryOptions);

                    // Register tracker.
                    _query.ContentsChanged += Query_ContentsChanged;

                    FilesFound = (int)await _query.GetItemCountAsync();
                }

                // Time and log file parsing.
                using (new DisposableLogger(GalleryLog.FileParseBegin, (sw) => GalleryLog.FileParseEnd(sw, FilesFound)))
                {
                    uint index = 0, stepSize = SettingsService.Instance.MediaLoadBatchSize;
                    var files = await _query.GetFilesAsync(index, stepSize);
                    index += stepSize;
                    while (files.Count != 0)
                    {
                        var fileTask = _query.GetFilesAsync(index, stepSize).AsTask();
                        for (var i = 0; i < files.Count; i++)
                        {
                            var mediaFile = files[i];

                            // Don't bother with files not supported by MIME.
                            if (!FileTypes.IsSupportedExtension(mediaFile.FileType))
                                continue;

                            if (mediaDatas == null)
                            {
                                await AddFileAsync(imageSize, mediaFile, false);
                            }
                            else
                            {
                                // Find mediaData in mediaDatas based on path of mediaFile.
                                var matchingMediaDatas = mediaDatas.Where(data => data?.Meta?.MediaFilePath?.Equals(mediaFile?.Path) ?? false).ToList();

                                var mediaData = matchingMediaDatas.FirstOrDefault();

                                if (mediaData is null)
                                {
                                    // Track the lost media files.
                                    _lostMediaFiles.Add(mediaFile);
                                    continue;
                                }
                                if (mediaData.Meta is null)
                                    continue;
                                if (string.IsNullOrEmpty(mediaData.Meta.MediaFilePath))
                                    continue;

                                // Tell the mediaData which file belongs to it.
                                mediaData.MediaFile = mediaFile;

                                await AddFileAsync(imageSize, mediaFile, false, mediaData);
                            }
                        }
                        files = await fileTask;
                        index += stepSize;
                        // Report progress to UI.
                        progress?.Report(MediaDatas.Count);
                    }
                }

                // Register tracker.
                RegisterFolderContentTracker();

                // Run tracker once to pick up on any file changes since last application boot.
                Query_ContentsChanged(_query, null);
            }
        }

        public Dictionary<MediaData, List<MediaData>> ScanForDuplicates()
        {
            var duplicates = new Dictionary<MediaData, List<MediaData>>();
            // Time and log scanning.
            using (new DisposableLogger(GalleryLog.DuplicateScanBegin, (sw) => GalleryLog.DuplicateScanEnd(sw, MediaDatas.Count, duplicates.Count)))
            {
                foreach (var mediaData in MediaDatas)
                {
                    ScanForDuplicate(duplicates, mediaData);
                }
            }
            return duplicates;
        }

        public void ScanForDuplicate(Dictionary<MediaData, List<MediaData>> duplicates, MediaData mediaData)
        {
            foreach (var otherMediaData in MediaDatas)
            {
                // Skip mediaData since it will be added anyways if there are any duplicates.
                if (otherMediaData == mediaData)
                    continue;

                if (otherMediaData.IsDuplicateOf(mediaData))
                {
                    // If otherMediaData is already located in duplicates, continue.
                    if (duplicates.Any(pair => pair.Value.Any(data => data == otherMediaData)))
                        continue;

                    if (duplicates.ContainsKey(mediaData))
                    {
                        // Add otherMediaData to duplicate collection of mediaData.
                        duplicates[mediaData].Add(otherMediaData);
                    }
                    else
                    {
                        // Create new duplicates list for mediaData and add otherMediaData and mediaData to it.
                        duplicates.Add(mediaData, new List<MediaData> { otherMediaData, mediaData });
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="StorageFile"/> to this gallery.
        /// </summary>
        /// <param name="imageSize">The size of the pregenerated thumbnail.</param>
        /// <param name="mediaFile">The file to add.</param>
        /// <param name="mediaData">Optional, the data of the file to add.</param>
        /// <returns></returns>
        public async Task AddFileAsync(int imageSize, StorageFile mediaFile, bool evaluate = true, MediaData data = null)
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
                    new string[] { "System.DateModified" }
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

            if (evaluate)
            {
                // Make sure local model is in memory.
                await LoadLocalModel();

                // Evaluate locally.
                await data.EvaluateLocalAsync(_memeClassifier);

                // Scan for duplicates before evaluating online.
                var duplicateSets = new Dictionary<MediaData, List<MediaData>>();
                ScanForDuplicate(duplicateSets, data);
                // For each set of duplicates found...
                foreach (var duplicates in duplicateSets.Values)
                {
                    // ...have the user handle the duplicates.
                    DuplicateModal.DuplicateQueue.Enqueue(duplicates);
                }
                // Process enqeued duplicates.
                await DuplicateModal.ProcessQueueAsync();

                // If data or mediafile was removed during duplication handling, then don't try to evaluate online.
                var fileName = Path.GetFileName(data?.MediaFile?.Path);
                var exists = await _folder.TryGetItemAsync(fileName);
                if (exists == null)
                    return;

                // Sign into firebase.
                if (await FirebaseClient.Client.SignInPromptUserIfNecessary())
                {
                    // Evaluate online.
                    await data.EvaluateOnlineAsync(FirebaseClient.accessToken);
                }
            }
        }

        /// <summary>
        /// Removes a <see cref="StorageFile"/> from this gallery.
        /// Note: Does not remove the file from the file system.
        /// </summary>
        /// <param name="mediaFilePath">The path of the file to remove.</param>
        public void RemoveFile(string mediaFilePath)
        {
            if (string.IsNullOrEmpty(mediaFilePath))
                return;

            var mediaDataToRemove = GetMediaDataFromPath(mediaFilePath, MediaDatas);
            if (mediaDataToRemove == null)
                return;

            var index = MediaDatas.IndexOf(mediaDataToRemove);
            if (index != -1)
                MediaDatas.RemoveAt(index);

            if (App.Files.Keys.Contains(mediaDataToRemove.Title))
                App.Files.Remove(mediaDataToRemove.Title);
            if (App.MediaDatas.Contains(mediaDataToRemove))
                App.MediaDatas.Remove(mediaDataToRemove);
        }

        /// <summary>
        /// Gets a <see cref="MediaData"/> from <see cref="MediaDatas"/> using the path of the media file
        /// as the identifying feature.
        /// </summary>
        /// <param name="mediaFilePath">The path of the file of the media data to retrieve.</param>
        /// <returns></returns>
        public MediaData GetMediaDataFromPath(string mediaFilePath, IEnumerable<MediaData> mediaDatasToLookIn)
        {
            MediaData mediaDataToRemove = null;
            foreach (var mediaData in mediaDatasToLookIn)
            {
                if (mediaData.Meta.MediaFilePath.Equals(mediaFilePath))
                {
                    mediaDataToRemove = mediaData;
                    break;
                }
            }

            return mediaDataToRemove;
        }

        /// <summary>
        /// Gets a <see cref="StorageFile"/> from <see cref="storageFilesToLookIn"/> using the path of the media file
        /// as the identifying feature.
        /// </summary>
        /// <param name="mediaFilePath">The path of the file of the media data to retrieve.</param>
        /// <returns></returns>
        public StorageFile GetMediaFileFromPath(string mediaFilePath, IEnumerable<StorageFile> storageFilesToLookIn)
        {
            StorageFile mediaDataToRemove = null;
            foreach (var mediaFile in storageFilesToLookIn)
            {
                if (mediaFile.Path.Equals(mediaFilePath))
                {
                    mediaDataToRemove = mediaFile;
                    break;
                }
            }

            return mediaDataToRemove;
        }

        public static Task ApplyThumbnails(int imageSize, StorageFile mediaFile, MediaData data)
        {
            //
            // Apply bitmap source depending on file type.
            //
            /*switch (mediaFile.FileType)
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
            }*/
            return Task.CompletedTask;
        }

        public async Task EvaluateImagesOnline(IProgress<int> progress, CancellationToken cancellationToken)
        {
            // Sign into firebase.
            if (await FirebaseClient.Client.SignInPromptUserIfNecessary())
            {
                // Find all media datas without annotation data.
                var mediaDatasWithoutAnnotationData = MediaDatas.Where(data => data.Meta.AnnotationData == null);

                var evaluated = 0;
                using (new DisposableLogger(GalleryLog.EvaluateOnlineBegin, (sw) => GalleryLog.EvaluateOnlineEnd(sw, evaluated)))
                {
                    IsEvaluating = true;
                    await mediaDatasWithoutAnnotationData.ForEachAsyncConcurrent(async media =>
                    {
                        try
                        {
                            // Evaluate media thumbnail online.
                            await media.EvaluateOnlineAsync(FirebaseClient.accessToken);
                        }
                        catch (Exception e)
                        {
                            // Log Exception.
                            LifecycleLog.Exception(e);
                        }
                        finally
                        {
                            // Update and then report progress.
                            evaluated++;
                            progress.Report(evaluated);
                        }
                    }, cancellationToken, 5);
                    IsEvaluating = false;
                }

                // Save online results to database.
                await DatabaseUtils.SaveAllMetadatasAsync(MediaDatas);
            }
        }

        public async Task EvaluateImagesLocally(IProgress<int> progress, CancellationToken cancellationToken)
        {
            var evaluated = 0;
            using (new DisposableLogger(GalleryLog.EvaluateLocalBegin, (sw) => GalleryLog.EvaluateLocalEnd(sw, evaluated)))
            {
                IsEvaluating = true;

                await LoadLocalModel();

                for (var i = 0; i < MediaDatas.Count; i++)
                {
                    var data = MediaDatas[i];

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
            }

            // Save local results to database in case online image evaluation never finishes.
            await DatabaseUtils.SaveAllMetadatasAsync(MediaDatas);
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

        private async void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            // Run on UI thread.
            await App.Current.NavigationService.Frame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {

                var tracker = _folder.TryGetChangeTracker();
                if (tracker != null)
                {
                    var changeReader = tracker.GetChangeReader();
                    var changes = await changeReader.ReadBatchAsync();

                    foreach (var change in changes)
                    {
                        if (change.ChangeType == StorageLibraryChangeType.ChangeTrackingLost)
                        {
                            // Change tracker is in an invalid state and must be reset
                            // This should be a very rare case, but must be handled
                            tracker.Reset();
                            return;
                        }
                        if (change.IsOfType(StorageItemTypes.File))
                        {
                            // Process file change on UI thread.
                            await ProcessFileChange(change);
                        }
                        else if (change.IsOfType(StorageItemTypes.Folder))
                        {
                            // No-op; not interested in folders
                        }
                        else
                        {
                            if (change.ChangeType == StorageLibraryChangeType.Deleted)
                            {
                                // TODO: The application does not have to support FAT drives at the moment. NTFS is enough.
                                //UnknownItemRemoved(change.Path);
                            }
                        }
                    }

                    // If any changes were recorded, save database.
                    if (changes.Count > 0)
                    {
                        // Log query contens changed saving.
                        GalleryLog.QueryContentsChangedSaving();
                        await DatabaseUtils.SaveAllMetadatasAsync(MediaDatas);
                    }

                    await changeReader.AcceptChangesAsync();
                }

            });
        }

        private async Task ProcessFileChange(StorageLibraryChange change)
        {
            // Temp variable used for instantiating StorageFiles for sorting if needed later
            //StorageFile newFile = null;
            var extension = Path.GetExtension(change.Path);

            switch (change.ChangeType)
            {
                // New File in the Library
                case StorageLibraryChangeType.Created:
                case StorageLibraryChangeType.MovedIntoLibrary:
                    if (FileTypes.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            // Don't add file to gallery if it already exists.
                            if (MediaDatas.Any(data => data.MediaFile.Path == change.Path))
                                return;

                            var mediaFile = (StorageFile)(await change.GetStorageItemAsync());

                            if (mediaFile == null)
                                return;

                            await AddFileAsync(ImageSize, mediaFile);
                        }
                        catch (Exception e)
                        {
                            // Log Exception.
                            LifecycleLog.Exception(e);
                        }
                    }
                    break;
                // Renamed file
                case StorageLibraryChangeType.MovedOrRenamed:
                    if (FileTypes.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            var mediaData = GetMediaDataFromPath(change.PreviousPath, MediaDatas);
                            var newName = Path.GetFileName(change.Path);

                            // If no MediaData could be found in Gallery for this renamed item AND there is an entry for it in App.MediaDatas (lostMediaData)
                            // that probably means it has been renamed while the application was off, so update its path and add it to the gallery.
                            var lostMediaFile = GetMediaFileFromPath(change.Path, _lostMediaFiles);
                            if (mediaData == null && lostMediaFile != null)
                            {
                                var lostMetaData = GetMediaMetaDataFromPath(change.PreviousPath, App.DatabaseMetaDatas);
                                // Update path on metadata.
                                lostMetaData.MediaFilePath = change.Path;
                                // Get current MediaData associated with metadata.
                                var lostMediaData = await MediaData.CreateFromMediaMetadataAsync(lostMetaData);
                                // If file can still not be found then return.
                                if (lostMediaData == null)
                                    return;
                                // Add file to gallery, including the lost media data.
                                await AddFileAsync(ImageSize, lostMediaFile, false, lostMediaData);
                            }
                            else if (mediaData == null)
                                return;

                            // Don't rename file in gallery if it is already renamed
                            if (MediaDatas.Any(data => data.Title == newName))
                                return;

                            var mediaFile = (StorageFile)(await change.GetStorageItemAsync());
                            // Run on UI thread.
                            await App.Current.NavigationService.Frame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                // Update MediaFile of mediaData.
                                mediaData.MediaFile = mediaFile;
                            });

                            await mediaData.UpdateFileNameValuesAsync(newName);
                        }
                        catch (Exception e)
                        {
                            // Log Exception.
                            LifecycleLog.Exception(e);
                        }
                    }
                    break;
                // File Removed From Library
                case StorageLibraryChangeType.Deleted:
                case StorageLibraryChangeType.MovedOutOfLibrary:
                    if (FileTypes.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            // Only remove file from gallery if it exists in gallery.
                            if (MediaDatas.Any(data => data.MediaFile.Path == change.Path))
                            {
                                RemoveFile(change.Path);
                            }
                        }
                        catch (Exception e)
                        {
                            // Log Exception.
                            LifecycleLog.Exception(e);
                        }
                    }
                    break;
                // Modified Contents
                case StorageLibraryChangeType.ContentsChanged:
                    if (FileTypes.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        /*newFile = (StorageFile)(await change.GetStorageItemAsync());
                        var imageProps = await newFile.Properties.GetImagePropertiesAsync();
                        var dateTaken = imageProps.DateTaken;
                        var dateModified = newFile.DateCreated;
                        if (DateTimeOffset.Compare(dateTaken.AddSeconds(70), dateModified) > 0)
                        {
                            // File was modified by the user
                        }*/
                    }
                    break;
                // Ignored Cases
                case StorageLibraryChangeType.EncryptionChanged:
                case StorageLibraryChangeType.ContentsReplaced:
                case StorageLibraryChangeType.IndexingStatusChanged:
                default:
                    // These are safe to ignore in this application
                    break;
            }
        }

        private void RegisterFolderContentTracker()
        {
            var tracker = _folder.TryGetChangeTracker();
            if (tracker != null)
            {
                tracker.Enable();
            }
        }

        private MediaMetadata GetMediaMetaDataFromPath(string mediaFilePath, IEnumerable<MediaMetadata> metaDatasToLookIn)
        {
            foreach (var mediaFile in metaDatasToLookIn)
            {
                if (mediaFile.MediaFilePath.Equals(mediaFilePath))
                {
                    return mediaFile;
                }
            }
            return null;
        }

        #endregion
    }
}
