using DMO.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;
using DMO.Services.SettingsServices;
using System.Net.Http;
using DMO.Utility;
using System.IO;
using System.Net.Http.Headers;
using System.Diagnostics;
using Template10.Mvvm;
using DMO.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using DMO.Controls;
using DMO.GoogleAPI;

namespace DMO.ViewModels
{
    public class GalleryPageViewModel : ViewModelBase
    {
        #region Private Members

        private MediaData _transitionItem;

        private const string predictionKey = "3addceda86d8415e88cb2074d7763920";
        private const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/59181889-0149-4bc1-845f-c70c6b1f6abd/image";
        private Gallery _gallery;

        #endregion

        #region Public Properties

        public Gallery Gallery
        {
            get => _gallery;
            private set
            {
                _gallery = value;
                _gallery.PropertyChanged += (sender, property) =>
                {
                    if (property.PropertyName == "FilesFound")
                    {
                        RaisePropertyChanged(nameof(LoadingProgressStatus));
                    }
                };
            }
        }

        public int TileSize { get; set; } = 256;

        public float EvaluationProgress { get; set; }

        public bool IsMediaLoading { get; set; } = true;

        public bool IsEvaluatingImages { get; set; } = false;

        public AdvancedCollectionView SearchResults { get; set; }

        public SortDirection DirectionSort
        {
            get => SettingsService.Instance.SortDirection;
            set
            {
                SettingsService.Instance.SortDirection = value;

                SortSelectionChanged();
            }
        }

        public string SortBy
        {
            get => SettingsService.Instance.SortBy;
            set => SettingsService.Instance.SortBy = value;
        }

        public string SearchQuery { get; set; }

        public string SearchPlaceHolderText
        {
            get
            {
                var options = new[]
                {
                    "Dank Memes",
                    "Rare Pepes",
                    "Your Mom",
                    "H E N T A I",
                    "E",
                };
                var random = new Random();
                var index = random.Next(0, options.Length);
                var searchFor = options[index];
                return $"Search for {searchFor}";
            }
        }

        public string LoadingProgressStatus => $"{Gallery?.FilesFound ?? 0} Dank Memes located so far...";

        #endregion

        #region Constructor

        public GalleryPageViewModel() : base()
        {

        }

        #endregion

        #region Public Methods

        public void SearchTextChanged()
        {
            IsMediaLoading = true;
            SearchResults.Filter = data => ((MediaData)data).Title.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant());
            SearchResults.RefreshFilter();
            IsMediaLoading = false;
        }

        public void SearchQuerySubmitted()
        {

        }

        public void SearchSuggestionChosen()
        {

        }

        public void ToggleSortDirection()
        {
            if (DirectionSort == SortDirection.Ascending)
                DirectionSort = SortDirection.Descending;
            else
                DirectionSort = SortDirection.Ascending;
        }

        public void SortSelectionChanged()
        {
            if (SearchResults?.SortDescriptions != null)
            {
                var sortDescription = GetSortDescription(SortBy);

                SearchResults.SortDescriptions.Clear();
                SearchResults.SortDescriptions.Add(sortDescription);
                SearchResults.RefreshSorting();
            }
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MediaData mediaData)
            {
                /*if (sender is GridView grid)
                {
                    _transitionItem = mediaData;

                    var container = grid.ContainerFromItem(mediaData) as GridViewItem;
                    //var subItem = Template10.Utils.XamlUtils.FirstChild<FastImage>(container);
                    //var image = Template10.Utils.XamlUtils.FirstChild<Image>(subItem);

                    if (mediaData is ImageData)
                        grid.PrepareConnectedAnimation("detailsImage1", mediaData, "FastImage");
                    if (mediaData is GifData)
                        grid.PrepareConnectedAnimation("detailsGif1", mediaData, "HoverGif");
                    if (mediaData is VideoData)
                        grid.PrepareConnectedAnimation("detailsVideo1", mediaData, "MediaPlayerHover");
                    
                    Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Navigating to {nameof(DetailsPage)}...");
                    await NavigationService.NavigateAsync(typeof(DetailsPage), mediaData.MediaFile.Name, new ContinuumNavigationTransitionInfo());
                    Debug.WriteLine($"{DateTime.Now.Second}:{DateTime.Now.Millisecond} Navigated to {nameof(DetailsPage)}!");
                }*/
                await mediaData.EvaluateOnlineAsync(FirebaseClient.accessToken);
            }
        }
        
        public async void GridViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_transitionItem != null)
            {
                if (sender is GridView grid)
                {
                    grid.ScrollIntoView(_transitionItem);
                    grid.UpdateLayout();

                    if (ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsImage2") is ConnectedAnimation imageAnim)
                        await grid.TryStartConnectedAnimationAsync(imageAnim, _transitionItem, "FastImage");
                    if (ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsGif2") is ConnectedAnimation gifAnim)
                        await grid.TryStartConnectedAnimationAsync(gifAnim, _transitionItem, "HoverGif");
                    if (ConnectedAnimationService.GetForCurrentView().GetAnimation("detailsVideo2") is ConnectedAnimation videoAnim)
                        await grid.TryStartConnectedAnimationAsync(videoAnim, _transitionItem, "MediaPlayerHover");
                }
            }
        }

        private DelegateCommand _goToSettingsPage;
        public DelegateCommand GoToSettingsPage
            => _goToSettingsPage ?? (_goToSettingsPage = new DelegateCommand(async () =>
            {
                await NavigationService.NavigateAsync(typeof(SettingsPage));
            }));

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (mode != NavigationMode.Back)
            {

                IsMediaLoading = true;
                if (state.Any())
                {
                    Gallery = new Gallery(state[nameof(Gallery)]?.ToString());
                    TileSize = (int)state[nameof(TileSize)];

                    // Load Gallery.
                    await LoadGallery();
                }
                else if (parameter is GalleryFolderChooser chooser)
                {
                    // Greate gallery from chooser result.
                    Gallery = new Gallery(chooser.FolderPath);
                    // Load Gallery.
                    await LoadGallery();
                }
                else if (parameter is string folderPath)
                {
                    // Greate gallery from folderPath.
                    Gallery = new Gallery(folderPath);
                    // Load Gallery.
                    await LoadGallery();
                }
                IsMediaLoading = false;

                IsEvaluatingImages = true;
                // Create progress object to report back evaluation progress.
                var progress = new Progress<int>();
                progress.ProgressChanged += (sender, evaluated) =>
                {
                    EvaluationProgress = ((float)evaluated / Gallery.ImageCount) * 100;
                };
                // Now use machine learning to automatically tag all images in gallery as a background task.
                await Gallery.EvaluateImages(progress);
                IsEvaluatingImages = false;

                string googleToken;
                if (await GoogleClient.Client.GetAccessTokenWithoutAuthentication())
                    googleToken = GoogleClient.accessToken;
                else
                    googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();
                Debug.WriteLine($"Google access token successfully aquired: {googleToken}");

                if (await FirebaseClient.Client.SignInWithFirebaseAsync(googleToken))
                {
                    Debug.WriteLine($"Firebase access token successfully aquired: {FirebaseClient.accessToken}");
                }
                else
                {
                    // Could not log into firebase. Could be caused by a refresh token revocation, try re-authenticating with Google.
                    googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();
                    // Retry firebase login.
                    if (await FirebaseClient.Client.SignInWithFirebaseAsync(googleToken))
                    {
                        Debug.WriteLine($"Firebase access token successfully aquired: {FirebaseClient.accessToken}");
                    }
                }
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            if (suspending)
            {
                pageState[nameof(Gallery)] = Gallery.RootFolderPath;
                pageState[nameof(TileSize)] = TileSize;
                foreach (var media in Gallery.MediaDatas)
                {
                    if (media is VideoData video)
                    {
                        video.Suspended = true;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            return base.OnNavigatingFromAsync(args);
        }

        #endregion

        #region Private Methods

        private SortDescription GetSortDescription(string sortBy)
        {
            var sortProperty = nameof(MediaData.Title);
            switch (sortBy)
            {
                case "Name":
                    sortProperty = nameof(MediaData.Title);
                    break;
                case "Last Modified":
                    sortProperty = nameof(MediaData.LastModified);
                    break;
                case "Created":
                    sortProperty = nameof(MediaData.Created);
                    break;
            }
            return new SortDescription(sortProperty, DirectionSort);
        }

        private async Task LoadGallery()
        {
            SearchResults = new AdvancedCollectionView(Gallery.MediaDatas, true);
            // Apply sort description.
            SearchResults.SortDescriptions.Add(GetSortDescription(SettingsService.Instance.SortBy));
            // Scan all media in gallery.
            using (SearchResults.DeferRefresh()) // Defer list updating until all items have been added.
            {
                // Load folder contents.
                await Gallery.LoadFolderContents(TileSize);
            }
        }

        private async Task MakeAnalysisWebRequest(StorageFile imageStorageFile)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                /*client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters =
                    "visualFeatures=Categories";*/

                // Assemble the URI for the REST API Call.
                string uri = uriBase/* + "?" + requestParameters*/;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = await GetImageAsByteArray(imageStorageFile);

                using (var content = new ByteArrayContent(byteData))
                {
                    // Authorize with key.
                    content.Headers.Add("Prediction-Key", predictionKey);

                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                Debug.WriteLine("\nResponse:\n");
                Debug.WriteLine(JsonPrettyPrint(contentString));
            }
            catch (Exception e)
            {
                Debug.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageStorageFile">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        private static async Task<byte[]> GetImageAsByteArray(StorageFile imageStorageFile)
        {
            using (var fileStream = await imageStorageFile.OpenStreamForReadAsync())
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            string INDENT_STRING = "    ";
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}
