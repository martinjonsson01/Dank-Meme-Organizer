using DMO.Utility;
using DMO.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Storage;
using Windows.System;

namespace DMO.Models
{
    public class DuplicateMediaEntry : BaseModel
    {
        public MediaData MediaData { get; set; }

        public string FolderPath => Path.GetDirectoryName(MediaData?.MediaFile?.Path) ?? "--";

        public string Size => ((long)MediaData?.BasicProperties?.Size).BytesToString() ?? "--";

        public string Dimensions => $"{MediaData?.Meta?.Width} x {MediaData?.Meta?.Height}";

        public string Added => MediaData?.Meta?.DateAdded.ToString("f", CultureInfo.InstalledUICulture) ?? "--";

        private DelegateCommand _openFolderCommand;
        public DelegateCommand OpenFolderCommand
            => _openFolderCommand ?? (_openFolderCommand = new DelegateCommand(async () =>
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(FolderPath);
                    if (folder == null) return;
                    var options = new FolderLauncherOptions
                    {
                        DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseMore
                    };
                    if (MediaData.MediaFile != null)
                        options.ItemsToSelect.Add(MediaData.MediaFile);
                    await Launcher.LaunchFolderAsync(folder, options);
                }
                catch (Exception e)
                {
                    // Log Exception.
                    LifecycleLog.Exception(e);
                }
            }));

        public DuplicateMediaEntry(MediaData mediaData)
        {
            MediaData = mediaData;
        }
    }
}
