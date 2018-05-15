using System.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using PropertyChanged;
using DMO.Services.SettingsServices;

namespace DMO.Models
{
    public class GalleryFolderChooser : BaseModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        /// <value>
        /// The folder path.
        /// </value>
        public string FolderPath { get; set; } = "Tell us where your Dank Memes are located...";

        /// <summary>
        /// Gets a value indicating whether a folder has been chosen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a folder has been selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsFolderChosen => FolderPath != "Tell us where your Dank Memes are located...";

        #endregion

        #region Constructor

        public GalleryFolderChooser()
        {

        }

        #endregion

        #region Public Methods

        public async Task ChooseFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("gallery", folder);
                // Update property.
                FolderPath = folder.Path;
                // Update settings.
                SettingsService.Instance.FolderPath = folder.Path;
            }
        }

        #endregion
    }
}
