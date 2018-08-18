using DMO.Database;
using DMO.Extensions;
using DMO.GoogleAPI;
using DMO.ML;
using DMO.Utility;
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
using Windows.UI.Xaml.Controls;

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
            set => TryRenameFileAsync(value);
        }
        
        public DateTimeOffset Created => MediaFile?.DateCreated ?? DateTimeOffset.Now;

        public DateTime LastModified => Meta?.LastModified.Date ?? DateTime.Now;

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
        /// <param name="newName">The new name of this file.</param>
        public async void TryRenameFileAsync(string newName)
        {
            if (MediaFile == null) return;
            if (string.IsNullOrEmpty(newName)) return;
            if (newName == Title) return;
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

            // Check if a file already exists at new path.
            var nameIndex = MediaFile.Path.Length - MediaFile.Name.Length;
            var folderPath = MediaFile.Path.Remove(nameIndex);
            var newPath = Path.Combine(folderPath, newName);
            if (await Task.Factory.StartNew(() => File.Exists(newPath))) return;

            // Rename file.
            await MediaFile.RenameAsync(newName);

            // Rename any media data values in memory.
            await UpdateFileNameValuesAsync(newName);
        }

        /// <summary>
        /// Updates any paths or names to this file in memory.
        /// NOTE: Does not rename the actual file on disk. Use <see cref="TryRenameFileAsync(string)"/> for that.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task UpdateFileNameValuesAsync(string newName)
        {
            var nameIndex = MediaFile.Path.Length - MediaFile.Name.Length;
            var folderPath = MediaFile.Path.Remove(nameIndex);
            var newPath = Path.Combine(folderPath, newName);

            // Update static list with new path.
            foreach (var mediaData in App.MediaDatas)
            {
                if (mediaData.Title == Title)
                    mediaData.Meta.MediaFilePath = newPath;
            }
            // Update path property.
            Meta.MediaFilePath = newPath;

            // Remove old file name from static dictionary.
            if (App.Files.ContainsKey(MediaFile.Name))
                App.Files.Remove(MediaFile.Name);

            // Add new file name to static dictionary.
            if (!App.Files.ContainsKey(newName))
                App.Files.Add(newName, MediaFile);
            else
                App.Files[newName] = MediaFile;

            // Run on UI thread.
            await App.Current.NavigationService.Frame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                // Trigger PropertyChanged event.
                RaisePropertyChanged(nameof(Title));
            });

            // Update database with new path.
            using (var context = new MediaMetaDatabaseContext())
            {
                await context.UpdateMediaMetaDataAsync(Meta);
            }
        }

        public static async Task DeleteAsync(StorageFile mediaFile, ContentDialog deleteConfirmDialog = null)
        {
            if (App.Gallery.IsEvaluating)
            {
                deleteConfirmDialog?.Hide();
                var errorDialog = new ContentDialog()
                {
                    Title = "Cannot delete file.",
                    Content = "Please wait for image evaluation to finish before deleting any files.",
                    PrimaryButtonText = "Ok",
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Remove from Gallery.
            App.Gallery.RemoveFile(mediaFile.Path);
            
            // Delete file from file system.
            await mediaFile.DeleteAsync();
        }

        public bool IsDuplicateOf(MediaData mediaData)
        {
            if (mediaData?.Meta?.Labels == null) return false;
            if (Meta?.Labels == null) return false;

            return Meta.Labels.ListEquals(mediaData.Meta.Labels);
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
#if !DEBUG
            var result = await CloudVisionClient.Client.SendFirebaseAnalyzeRequest(imageStream, firebaseToken);
#endif
#if DEBUG
            var result = "[{\"faceAnnotations\":[],\"landmarkAnnotations\":[],\"logoAnnotations\":[],\"labelAnnotations\":[],\"textAnnotations\":[],\"safeSearchAnnotation\":{\"adult\":\"UNLIKELY\",\"spoof\":\"UNLIKELY\",\"medical\":\"UNLIKELY\",\"violence\":\"VERY_UNLIKELY\",\"racy\":\"LIKELY\"},\"imagePropertiesAnnotation\":{\"dominantColors\":{\"colors\":[{\"color\":{\"red\":32,\"green\":27,\"blue\":25,\"alpha\":null},\"score\":0.28402772545814514,\"pixelFraction\":0.3229365050792694},{\"color\":{\"red\":113,\"green\":82,\"blue\":73,\"alpha\":null},\"score\":0.08688181638717651,\"pixelFraction\":0.10325396806001663},{\"color\":{\"red\":56,\"green\":47,\"blue\":41,\"alpha\":null},\"score\":0.2374236285686493,\"pixelFraction\":0.22912698984146118},{\"color\":{\"red\":95,\"green\":82,\"blue\":75,\"alpha\":null},\"score\":0.07915570586919785,\"pixelFraction\":0.06817460060119629},{\"color\":{\"red\":108,\"green\":79,\"blue\":75,\"alpha\":null},\"score\":0.07438167929649353,\"pixelFraction\":0.06182539835572243},{\"color\":{\"red\":103,\"green\":85,\"blue\":72,\"alpha\":null},\"score\":0.04776318743824959,\"pixelFraction\":0.03325396776199341},{\"color\":{\"red\":74,\"green\":49,\"blue\":47,\"alpha\":null},\"score\":0.0431894026696682,\"pixelFraction\":0.03214285895228386},{\"color\":{\"red\":64,\"green\":51,\"blue\":39,\"alpha\":null},\"score\":0.03133033961057663,\"pixelFraction\":0.02309523895382881},{\"color\":{\"red\":140,\"green\":108,\"blue\":106,\"alpha\":null},\"score\":0.02590302750468254,\"pixelFraction\":0.018174603581428528},{\"color\":{\"red\":82,\"green\":53,\"blue\":45,\"alpha\":null},\"score\":0.022915206849575043,\"pixelFraction\":0.017936507239937782}]}},\"error\":null,\"cropHintsAnnotation\":{\"cropHints\":[{\"boundingPoly\":{\"vertices\":[{\"x\":0,\"y\":0},{\"x\":1279,\"y\":0},{\"x\":1279,\"y\":719},{\"x\":0,\"y\":719}],\"normalizedVertices\":[]},\"confidence\":0.5999999642372131,\"importanceFraction\":1.4199999570846558}]},\"fullTextAnnotation\":null,\"webDetection\":{\"webEntities\":[{\"entityId\":\"/m/01vw8mh\",\"score\":1.315500020980835,\"description\":\"Snoop Dogg\"},{\"entityId\":\"/m/03bfb\",\"score\":0.703000009059906,\"description\":\"GIF\"},{\"entityId\":\"/g/11cs552z6v\",\"score\":0.612500011920929,\"description\":\"Gfycat\"},{\"entityId\":\"/m/02pn8wm\",\"score\":0.5238000154495239,\"description\":\"The Graham Norton Show\"},{\"entityId\":\"/m/04wpw\",\"score\":0.43160000443458557,\"description\":\"Meme\"},{\"entityId\":\"/m/0rz56gw\",\"score\":0.41929998993873596,\"description\":\"\"},{\"entityId\":\"/m/0f2f9\",\"score\":0.4020000100135803,\"description\":\"Television show\"},{\"entityId\":\"/m/0b2334\",\"score\":0.2847000062465668,\"description\":\"Reddit\"},{\"entityId\":\"/t/238xklxr0wv6f\",\"score\":0.27300000190734863,\"description\":\"\"},{\"entityId\":\"/m/0sy9b\",\"score\":0.26829999685287476,\"description\":\"Video clip\"}],\"fullMatchingImages\":[{\"url\":\"https://coubsecure-s.akamaihd.net/get/b146/p/coub/simple/cw_timeline_pic/bbf70f8743c/fea0911e3dead8b28ba0b/big_1483678715_image.jpg\",\"score\":0}],\"partialMatchingImages\":[{\"url\":\"https://image.krasview.ru/video/b6f6ffa6211b615/870.jpg\",\"score\":0},{\"url\":\"https://thumbs.gfycat.com/ClosedJampackedHorseshoecrab-size_restricted.gif\",\"score\":0},{\"url\":\"https://cdn2.kontraband.com/uploads/image/2017/05/24/11/010af116-8856-48c8-bf6c-e2c522bfcf22.gif\",\"score\":0},{\"url\":\"https://www.kontraband.com/uploads/image/2017/05/24/11/preview_010af116-8856-48c8-bf6c-e2c522bfcf22.gif\",\"score\":0},{\"url\":\"https://thumbs.gfycat.com/BlushingEcstaticAfricancivet-size_restricted.gif\",\"score\":0},{\"url\":\"https://cs10.pikabu.ru/images/big_size_comm_an/2018-07_3/153131287016905304.gif\",\"score\":0},{\"url\":\"https://giant.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker.gif\",\"score\":0},{\"url\":\"https://thumbs.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker-poster.jpg\",\"score\":0},{\"url\":\"https://2static1.fjcdn.com/thumbnails/comments/In+my+opinion+as+long+as+people+can+get++_e3f754bb026e58962b9e86fdab1d7ae9.jpg\",\"score\":0},{\"url\":\"https://pics.me.me/thumb_ri7pal-shit-2991139.png\",\"score\":0}],\"pagesWithMatchingImages\":[{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://thumbs.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker-poster.jpg\",\"score\":0}],\"url\":\"http://homdor.com/search/snoop-dogg-lion-raggea-weed\",\"score\":0,\"pageTitle\":\"<b>Snoop Dogg</b> Lion Raggea <b>Weed Gifs</b> Search | Search &amp; Share on ...\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://giant.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker.gif\",\"score\":0},{\"url\":\"https://thumbs.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker-poster.jpg\",\"score\":0}],\"url\":\"https://gfycat.com/gifs/detail/unconsciousincompatibleivorybilledwoodpecker\",\"score\":0,\"pageTitle\":\"<b>Is the weed on the show real</b> ? | Find, Make &amp; Share Gfycat <b>GIFs</b>\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://thumbs.gfycat.com/UnconsciousIncompatibleIvorybilledwoodpecker-max-1mb.gif\",\"score\":0}],\"url\":\"https://gfycat.com/gifs/search/snoop+dogg+weed\",\"score\":0,\"pageTitle\":\"<b>snoop dogg weed GIFs</b> | Find, Make &amp; Share Gfycat <b>GIFs</b>\"},{\"fullMatchingImages\":[{\"url\":\"https://coubsecure-s.akamaihd.net/get/b146/p/coub/simple/cw_timeline_pic/bbf70f8743c/fea0911e3dead8b28ba0b/big_1483678715_image.jpg\",\"score\":0}],\"partialMatchingImages\":[],\"url\":\"https://coub.com/view/mlp5n\",\"score\":0,\"pageTitle\":\"<b>Snoop Dogg</b> To Be Continued - Coub - <b>GIFs</b> with sound\"},{\"fullMatchingImages\":[{\"url\":\"https://coubsecure-s.akamaihd.net/get/b22/p/coub/simple/cw_timeline_pic/58f7050df0b/566cab381bd68fed71a41/big_1480643683_image.jpg\",\"score\":0}],\"partialMatchingImages\":[],\"url\":\"https://coub.com/view/i5k0m\",\"score\":0,\"pageTitle\":\"<b>Snoop Dogg</b> To Be Continued - Coub - <b>GIFs</b> with sound\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://pics.me.me/thumb_ri7pal-shit-2991139.png\",\"score\":0}],\"url\":\"https://me.me/t/dank-memes?since=1467745299%2C1248052\",\"score\":0,\"pageTitle\":\"Dank Memes\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://pics.me.me/thumb_ri7pal-shit-2991139.png\",\"score\":0}],\"url\":\"https://me.me/t/dank-memes?since=1467775053%2C1255546\",\"score\":0,\"pageTitle\":\"Dank Memes\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://cs10.pikabu.ru/images/big_size_comm_an/2018-07_3/153131287016905304.gif\",\"score\":0}],\"url\":\"https://pikabu.ru/story/snup_dogg_pro_zapis_alboma_bush_s17e07__yefir_22_maya_2015_6021012\",\"score\":0,\"pageTitle\":\"Снуп Догг про запись альбома Bush [s17e07] | Эфир 22 мая 2015\"},{\"fullMatchingImages\":[],\"partialMatchingImages\":[{\"url\":\"https://image.krasview.ru/video/b6f6ffa6211b615/870.jpg\",\"score\":0}],\"url\":\"http://hlamer.ru/video/538041--Trava_v_klipah_nastoyaschaya-net\",\"score\":0,\"pageTitle\":\"- Трава в клипах настоящая? - ...нет :) смотреть онлайн / КРЫША ...\"}],\"visuallySimilarImages\":[{\"url\":\"http://s4.storage.akamai.coub.com/get/b67/p/coub/simple/cw_image/55c07f28cba/05f6988ef556dfdc5914b/med_1475596827_00020.jpg\",\"score\":0},{\"url\":\"https://i.ytimg.com/vi/8lwOLjJA13k/hqdefault.jpg\",\"score\":0},{\"url\":\"https://coubsecure-s.akamaihd.net/get/b31/p/coub/simple/cw_timeline_pic/2f6738a17f3/17d131ff0f7c9bb7fae5e/big_1430595482_image.jpg\",\"score\":0},{\"url\":\"https://i.kym-cdn.com/photos/images/newsfeed/000/288/823/432.gif\",\"score\":0}],\"bestGuessLabels\":[{\"label\":\"snoop dogg is the weed on the show real gif\",\"languageCode\":\"en\"}]},\"context\":null}]";
#endif
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
