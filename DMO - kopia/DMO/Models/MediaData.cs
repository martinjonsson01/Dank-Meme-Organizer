using DMO.Database;
using DMO.GoogleAPI;
using DMO.GoogleAPI.Models;
using DMO.ML;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DMO.Models
{
    public abstract class MediaData : BaseModel
    {
        #region Public Properties
        
        public List<Label> Labels { get; set; }
        
        public StorageFile MediaFile { get; set; }

        public AnnotateImageReponse AnnotationData { private get; set; }

        public List<EntityAnnotation> TextAnnotations => AnnotationData?.TextAnnotations;

        public SafeSearchAnnotation SafeSearch => AnnotationData?.SafeSearchAnnotation;

        public List<ColorInfo> DominantColors => AnnotationData?.ImagePropertiesAnnotation?.DominantColors?.Colors;

        public WebDetection WebDetection => AnnotationData?.WebDetection;

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

        public DateTime LastModified { get; set; }

        public DateTimeOffset Created => MediaFile?.DateCreated ?? DateTimeOffset.Now;

        #endregion

        #region Constructor

        public MediaData(StorageFile file)
        {
            MediaFile = file;
        }

        #endregion

        #region Public Methods

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

        public async Task<JsonMediaData> ToJsonMediaData()
        {
            return new JsonMediaData
            {
                ID = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Labels, Formatting.Indented)),
                Json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(this))
            };
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
            AnnotationData = imageReponse;
        }

        /// <summary>
        /// Evaluates this Media using the local machine learning model and stores the results in this object.
        /// </summary>
        /// <param name="model">The local ML model to use when evaluating.</param>
        /// <returns></returns>
        public async Task EvaluateLocalAsync(MemeClassifierModel model)
        {
            // Create input using media thumbnail.
            var input = await CreateMLInput();

            // Evaluate input using local model...
            var output = await model.EvaluateAsync(input);

            // Order tags by descending loss, turn them into Label objects, and take the top 5.
            var tagsAndLoss = output.Loss.OrderByDescending(pair => pair.Value)
                .Select(pair => new Label { Name = pair.Key, Probability = pair.Value }).Take(5).ToList();
            // Update Tags. TODO: Put tags in a persistent database.
            Labels = tagsAndLoss;
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
