using DMO.Extensions;
using DMO.Models;
using DMO.Utility;
using DMO.Utility.Logging;
using DMO.Views;
using DMO_Model.GoogleAPI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.ViewModels
{
    public class DuplicatePageViewModel : ViewModelBase
    {
        #region Private Members

        private ContentDialog _selectConfirmDialog;

        #endregion

        #region Public Properties

        public ObservableCollection<DuplicateMediaEntry> DuplicateMediaEntries { get; set; }

        public TaskCompletionSource<string> DuplicateCompletionSource { get; set; }

        #endregion

        #region Public Methods

        public async void DuplicateSelected(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is DuplicateMediaEntry duplicateEntry)
            {
                // Prompt user for confirmation.
                _selectConfirmDialog = new ContentDialog()
                {
                    Title = "Delete all other duplicates?",
                    Content = "Cannot be undone.",
                    PrimaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary,
                    CloseButtonText = "Delete",
                    CloseButtonCommand = DuplicateSelectCommand,
                    CloseButtonCommandParameter = duplicateEntry,
                };

                await _selectConfirmDialog?.ShowAsync();
            }
        }

        #endregion

        #region Commands

        public DelegateCommand<DuplicateMediaEntry> DuplicateSelectCommand
            => new DelegateCommand<DuplicateMediaEntry>(async duplicateEntry =>
            {
                // Get all entries that are not the clicked item.
                var removeEntries = DuplicateMediaEntries.Where(entry => entry != duplicateEntry);

                // Remove all duplicate entries.
                foreach(var removeEntry in removeEntries)
                {
                    // Ignore entries with null paths.
                    if (string.IsNullOrEmpty(removeEntry?.MediaData?.MediaFile?.Path)) continue;

                    try
                    {
                        // Remove entry.
                        await MediaData.DeleteAsync(removeEntry.MediaData.MediaFile, _selectConfirmDialog);
                    }
                    catch (Exception e)
                    {
                        // Log Exception.
                        LifecycleLog.Exception(e);
                    }
                }

                // Set result.
                DuplicateCompletionSource.TrySetResult(string.Empty);
            });

        public DelegateCommand KeepAllDuplicatesCommand
            => new DelegateCommand(() =>
            {
                // Set result.
                DuplicateCompletionSource.TrySetResult(string.Empty);
            });

        #endregion

        #region Constructor

        public DuplicatePageViewModel()
        {

        }

        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(DuplicateMediaEntries)] = DuplicateMediaEntries;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
