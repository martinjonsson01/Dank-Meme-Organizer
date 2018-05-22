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
using Template10.Mvvm;
using DMO.Views;

namespace DMO.ViewModels
{
    public class GalleryPageViewModel : ViewModelBase
    {
        private Gallery _gallery;
        #region Public Properties

        public Gallery Gallery {
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

        public bool IsMediaLoading { get; set; } = true;

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
                await Gallery.ScanFolderContents(TileSize);
            }
        }

        #endregion
    }
}
