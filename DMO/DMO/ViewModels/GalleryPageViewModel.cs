using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMO.Models;
using DMO.Services.SettingsServices;
using DMO.Utility;
using DMO.Utility.Logging;
using DMO.Views;
using Microsoft.Toolkit.Uwp.UI;
using Template10.Mvvm;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class GalleryPageViewModel : ViewModelBase
    {
        #region Private Members

        private MediaData _transitionItem;

        private const string predictionKey = "3addceda86d8415e88cb2074d7763920";
        private const string uriBase = "https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/59181889-0149-4bc1-845f-c70c6b1f6abd/image";
        private Gallery _gallery;

        private CancellationTokenSource _evaluationCancellationTokenSource = new CancellationTokenSource();

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

        public bool IsProgressVisible { get; set; } = false;

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

        public string SearchQuery { get; set; } = string.Empty;

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
            {
                DirectionSort = SortDirection.Descending;
            }
            else
            {
                DirectionSort = SortDirection.Ascending;
            }
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
                if (sender is GridView grid)
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

                    using (new DisposableLogger(() => NavigationLog.NavBegin(nameof(DetailsPage)), (sw) => NavigationLog.NavEnd(sw, nameof(DetailsPage))))
                    {
                        await NavigationService.NavigateAsync(typeof(DetailsPage), mediaData.MediaFile.Name, new SuppressNavigationTransitionInfo());
                    }
                }
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
                // Create new token source.
                _evaluationCancellationTokenSource = new CancellationTokenSource();

                IsMediaLoading = true;
                if (state.Any())
                {
                    Gallery = new Gallery(state[nameof(Gallery)]?.ToString());
                    TileSize = (int)state[nameof(TileSize)];

                    // Load Gallery.
                    await LoadGalleryFromFolderAsync();
                }
                else if (parameter is GalleryFolderChooser chooser)
                {
                    // Greate gallery from folderPath.
                    Gallery = new Gallery(chooser.FolderPath);
                    // Load Gallery.
                    await LoadGalleryFromFolderAsync();
                }
                else if (parameter is string mediaDatasJson && mediaDatasJson == nameof(App.MediaDatas))
                {
                    // Greate gallery from mediaDatas.
                    Gallery = new Gallery();
                    // Load Gallery.
                    await LoadGalleryFromFolderAsync(App.MediaDatas);
                }
                else // Default behaviour.
                {
                    // Throw error if folderpath is null.
                    if (string.IsNullOrEmpty(SettingsService.Instance.FolderPath))
                    {
                        throw new ArgumentNullException($"{nameof(SettingsService.Instance.FolderPath)} is null. Could not create gallery.");
                    }

                    // Greate gallery from folderPath.
                    Gallery = new Gallery(SettingsService.Instance.FolderPath);
                    // Load Gallery.
                    await LoadGalleryFromFolderAsync();
                }
                IsMediaLoading = false;

                IsProgressVisible = true;
                // Create progress object to report back evaluation progress.
                var progress = new Progress<int>();
                progress.ProgressChanged += (sender, evaluated) => { EvaluationProgress = ((float)evaluated / Math.Max(Gallery.MediaDatas.Count, 0)) * 100; };
                try
                {
                    // Now use machine learning to automatically tag all images in gallery as a background task.
                    await Gallery.EvaluateImagesLocally(progress, _evaluationCancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    // Task was cancelled.
                    LifecycleLog.Exception(e);

                    // Save local results to database.
                    await DatabaseUtils.SaveAllMetadatasAsync(Gallery.MediaDatas);
                }

                // Scan for duplicates.
                var duplicateSets = Gallery.ScanForDuplicates();
                // For each set of duplicates...
                foreach (var duplicates in duplicateSets.Values)
                {
                    // ...have the user handle the duplicates.
                    DuplicateModal.DuplicateQueue.Enqueue(duplicates);
                }
                // Process duplicates one after another.
                await DuplicateModal.ProcessQueueAsync();

                // Create new progress object to report back evaluation progress.
                progress = new Progress<int>();
                progress.ProgressChanged += (sender, evaluated) => { EvaluationProgress = ((float)evaluated / Math.Max(Gallery.MediaDatas.Count, 0)) * 100; };
                try
                {
                    // Use Google Cloud Vision to evaluate media in gallery online.
                    await Gallery.EvaluateImagesOnline(progress, _evaluationCancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    // Task was cancelled.
                    LifecycleLog.Exception(e);

                    // Save online results to database.
                    await DatabaseUtils.SaveAllMetadatasAsync(Gallery.MediaDatas);
                }
                finally
                {
                    IsProgressVisible = false;
                }
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
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

                using (var session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.SavingData })
                {
                    // Request extension. This is done so that if the application can finish saving all data
                    // to the database when being suspended.
                    var result = await session.RequestExtensionAsync();
                    LifecycleLog.ExtensionRequestResult(result);

                    // Save all metadatas asynchronously before suspending.
                    await DatabaseUtils.SaveAllMetadatasAsync(Gallery.MediaDatas);
                }
            }
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
                    sortProperty = nameof(MediaData.Meta.LastModified);
                    break;
                case "Created":
                    sortProperty = nameof(MediaData.Created);
                    break;
            }
            return new SortDescription(sortProperty, DirectionSort);
        }

        private async Task LoadGalleryFromFolderAsync(ICollection<MediaData> mediaDatas = null)
        {
            SearchResults = new AdvancedCollectionView(Gallery.MediaDatas, true);
            // Apply sort description.
            SearchResults.SortDescriptions.Add(GetSortDescription(SettingsService.Instance.SortBy));
            // Scan all media in gallery.
            //using (SearchResults.DeferRefresh()) // Defer list updating until all items have been added.
            //{

            // Create new progress object to report back loading progress.
            var progress = new Progress<int>();
            progress.ProgressChanged += (sender, loaded) => 
            {
                EvaluationProgress = ((float)loaded / Gallery.FilesFound) * 100;
                // If any progress has been made.
                if (EvaluationProgress > 0)
                    IsMediaLoading = false; // Hide loading indicator.
            };
            IsProgressVisible = true;
            // Load folder contents.
            await Gallery.LoadFolderContents(TileSize, progress, mediaDatas);
            IsProgressVisible = false;
            //}
            // Update static list.
            App.MediaDatas = Gallery.MediaDatas.ToList();
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
                var binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        #endregion
    }
}
