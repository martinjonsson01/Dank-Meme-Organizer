using System;
using System.ComponentModel;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public abstract class MediaData : BaseModel
    {
        public StorageFile MediaFile;

        /// <summary>
        /// Gets or sets the title of this Media.
        /// </summary>
        /// <value>
        /// The title of this Media.
        /// </value>
        public string Title
        {
            get => MediaFile?.Name;
            set => RenameFile(value);
        }

        public async void RenameFile(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (value == Title) return;
            if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return;

            var nameIndex = MediaFile.Path.Length - MediaFile.Name.Length;
            var folderPath = MediaFile.Path.Remove(nameIndex);
            var combinedPath = Path.Combine(folderPath, value);
            if (File.Exists(combinedPath)) return;

            await MediaFile.RenameAsync(value);
            OnPropertyChanged(nameof(Title));
        }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        /// <value>
        /// The thumbnail.
        /// </value>
        public BitmapImage Thumbnail { get; set; }

        public DateTime LastModified { get; set; }

        public DateTimeOffset Created => MediaFile.DateCreated;

        public MediaData(StorageFile file, BitmapImage thumbnail)
        {
            MediaFile = file;
            Thumbnail = thumbnail;
        }

        public override bool Equals(object obj)
        {
            if (obj is MediaData data)
            {
                if (data.Title == Title)
                {
                    if (data.Thumbnail.Equals(Thumbnail))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
