using DMO.Extensions;
using DMO.GoogleAPI;
using DMO.ML;
using DMO_Model.GoogleAPI.Models;
using DMO_Model.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace DMO.Models
{
    public abstract class MediaData : BaseModel
    {
        #region Public Properties
        
        [JsonIgnore]
        public StorageFile MediaFile { get; set; }

        [JsonIgnore]
        public BasicProperties BasicProperties { get; set; }

        public MediaMetadata Meta { get; set; } = new MediaMetadata();

        public List<EntityAnnotation> TextAnnotations => Meta?.AnnotationData?.TextAnnotations;

        public SafeSearchAnnotation SafeSearch => Meta?.AnnotationData?.SafeSearchAnnotation;

        public List<ColorInfo> DominantColors => Meta?.AnnotationData?.ImagePropertiesAnnotation?.DominantColors?.Colors;

        public WebDetection WebDetection => Meta?.AnnotationData?.WebDetection;

        public bool Evaluating { get; set; }

        /// <summary>
        /// Gets or sets the title of this Media.
        /// </summary>
        /// <value>
        /// The title of this Media.
        /// </value>
        public string Title
        {
            get => MediaFile?.Name;
            set => TryRenameFile(value);
        }
        
        public DateTimeOffset Created => MediaFile?.DateCreated ?? DateTimeOffset.Now;

        #endregion

        #region Constructor

        public MediaData(StorageFile file)
        {
            MediaFile = file;
            Meta.MediaFilePath = file.Path;
        }

        /// <summary>
        /// To be used by EntityFramework and Serialization only.
        /// </summary>
        public MediaData()
        {

        }

        #endregion

        #region Public Methods

        public static async Task<MediaData> CreateFromMediaMetadataAsync(MediaMetadata metadata)
        {
            StorageFile mediaFile = null;
            try
            {
                // Try to load file from path. If not successful, the file might have been deleted, moved, or renamed.
                // In the case that it has been renamed, it should be scanned by the local ML and recognized as already existing
                // in the database, in which case it will update the path listed there.
                mediaFile = await StorageFile.GetFileFromPathAsync(metadata.MediaFilePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return null;
            }
            // Sanity check.
            if (mediaFile == null) return null;

            // Create instance of MediaData descendant depending on file type.
            var mediaData = CreateFromStorageFile(mediaFile);

            // Assign metadata object to MediaData.
            mediaData.Meta = metadata;

            return mediaData;
        }

        public static MediaData CreateFromStorageFile(StorageFile mediaFile)
        {
            //
            // Instantiate MediaData type depending on FileType.
            //
            if (mediaFile.FileType == ".gif")
            {
                return new GifData(mediaFile);
            }
            else if (mediaFile.IsVideo())
            {
                return new VideoData(mediaFile);
            }
            else
            {
                return new ImageData(mediaFile);
            }
        }

        /// <summary>
        /// Tries to rename this file to the provided value.
        /// </summary>
        /// <param name="value">The new name of this file.</param>
        public async void TryRenameFile(string value)
        {
            if (MediaFile == null) return;
            if (string.IsNullOrEmpty(value)) return;
            if (value == Title) return;
            if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

            var nameIndex = MediaFile.Path.Length - MediaFile.Name.Length;
            var folderPath = MediaFile.Path.Remove(nameIndex);
            var combinedPath = Path.Combine(folderPath, value);
            if (await Task.Factory.StartNew(() => File.Exists(combinedPath))) return;

            App.Files.Remove(MediaFile.Name);
            await MediaFile.RenameAsync(value);
            App.Files.Add(value, MediaFile);
            OnPropertyChanged(nameof(Title));
        }
        
        /// <summary>
        /// Gets the thumbnail of this media. Returns one of the first frames of video medias and GIFs.
        /// </summary>
        /// <returns>one of the first frames of video medias and GIFs</returns>
        public abstract Task<IRandomAccessStream> GetThumbnailAsync();

        /// <summary>
        /// Evaluates this Media using Google Cloud Vision and stores the results in this object.
        /// </summary>
        /// <param name="firebaseToken">The OAuth access token for firebase.</param>
        public async Task EvaluateOnlineAsync(string firebaseToken)
        {
            // Run on UI thread.
            await App.Current.NavigationService.Frame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                Evaluating = true;
            });

            var sw = new Stopwatch();
            sw.Start();
            var imageStream = await GetThumbnailAsync();
            var result = await CloudVisionClient.Client.SendFirebaseAnalyzeRequest(imageStream, firebaseToken);
            sw.Stop();
            Debug.WriteLine($"Cloud vision request completed! Waited {sw.ElapsedMilliseconds} ms for response");

            sw.Reset();
            sw.Start();
            // This is used to trim away the JSON array surrounding the response object.
            // This is done because the response will never contain more than one response object.
            var trimChars = new char[] { '[', ']' };
            // Deserialize JSON on another thread.
            var imageReponse = await Task.Run(() => JsonConvert.DeserializeObject<AnnotateImageReponse>(result.Trim(trimChars)));
            sw.Stop();
            Debug.WriteLine($"Response deserialization completed! Took {sw.ElapsedMilliseconds} ms");

            // Assign response data to this object.
            Meta.AnnotationData = imageReponse;

            // Run on UI thread.
            await App.Current.NavigationService.Frame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                Evaluating = false;
            });
        }

        /// <summary>
        /// Evaluates this Media using the local machine learning model and stores the results in this object.
        /// </summary>
        /// <param name="model">The local ML model to use when evaluating.</param>
        /// <returns></returns>
        public async Task EvaluateLocalAsync(MemeClassifierModel model)
        {
            Evaluating = true;

            // Create input using media thumbnail.
            var input = await CreateMLInput();

            // Evaluate input using local model...
            var output = await model.EvaluateAsync(input);

            // Order tags by descending loss, turn them into Label objects, and take the top 5.
            var orderedByDescending = output.Loss.OrderByDescending(pair => pair.Value);
            var tagsAndLoss = orderedByDescending.Select(pair => new Label { Name = pair.Key, Probability = pair.Value }).Take(Math.Min(5, orderedByDescending.Count())).ToList();
            // Update Tags.
            Meta.Labels = new ObservableCollection<Label>(tagsAndLoss);

            Evaluating = false;
        }

        #endregion

        #region Private Methods

        private async Task<MemeClassifierModelInput> CreateMLInput()
        {
            var input = new MemeClassifierModelInput();

            SoftwareBitmap softwareBitmap;
            using (var imageStream = await GetThumbnailAsync())
            {
                // Create the decoder from the stream 
                var decoder = await BitmapDecoder.CreateAsync(imageStream);

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                // Apply data to input.
                input.Data = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            }

            return input;
        }

        #endregion
    }
}
